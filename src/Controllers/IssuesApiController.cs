namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Web.Security;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public class IssuesApiController : ControllerBase
    {
        public override void Execute()
        {
            string handlerPath = this.Context.ApplicationPath + "api/";
#if DEBUG
            if (!this.Context.Url.AbsolutePath.WithTrailingSlash().StartsWithIgnoreCase(handlerPath))
            {
                throw new ApplicationException(String.Format("Paths are out of sync. Expected URL path to start with {0} but found {1}", handlerPath, this.Context.Url.AbsolutePath));
            }
#endif
            string path = this.Context.Url.AbsolutePath.WithTrailingSlash().Substring(handlerPath.Length).TrimEnd('/');
            Query q = QueryService.ParseQuery(this.Context.QueryString);

            long issueId = 0;
            if (!long.TryParse(path, out issueId))
            {
                path = null;
            }

            if (issueId == 0)
            {
                switch (this.Context.Method)
                {
                    case "GET":
                        {
                            QueriedIssues issues = QueryService.QueryIssues(q);
                            var pagePrefix = handlerPath + QueryService.RecreateQueryString(q) + "&page=";

                            IssuesApiViewModel vm = new IssuesApiViewModel()
                            {
                                Issues = issues.Issues,
                                Page = new Pagination(q.Page, q.Count, issues.Total, pagePrefix),
                            };
                            JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                        }
                        break;

                    case "POST":
                        {
                            if (!this.Context.Authenticated)
                            {
                                FormsAuthentication.RedirectToLoginPage();
                            }
                            else
                            {
                                CompleteIssue ci = null;
                                Issue i = CreateIssueFromCollection(this.Context.User, this.Context.Form);
                                QueryService.TryGetIssueWithComments(i.Id, out ci);
                                if (String.IsNullOrEmpty(q.Template))
                                {
                                    IssueApiViewModel vm = new IssueApiViewModel() { Issue = ci };
                                    JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                                }
                                else
                                {
                                    var template = FileService.LoadTemplate(q.Template);
                                    template.Render(ci, this.Context.GetOutput(), null);
                                }
                            }
                        }
                        break;

                    default:
                        this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                        break;
                }
            }
            else
            {
                CompleteIssue issue;
                if (!QueryService.TryGetIssueWithComments(issueId, out issue))
                {
                    this.Context.SetStatusCode(HttpStatusCode.NotFound);
                }
                else
                {
                    IssueApiViewModel vm = null;
                    switch (this.Context.Method)
                    {
                        case "GET":
                            vm = new IssueApiViewModel()
                            {
                                Issue = issue,
                            };
                            break;

                        case "PUT":
                        case "POST":
                            UpdateIssueFromCollection(this.Context.User, issue.Id, this.Context.Form);
                            QueryService.TryGetIssueWithComments(issueId, out issue);
                            vm = new IssueApiViewModel()
                            {
                                Issue = issue,
                            };
                            break;

                        case "DELETE":
                            DeleteIssue(issue.Id);
                            break;

                        default:
                            this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                            break;
                    }

                    if (vm != null)
                    {
                        JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                    }
                }
            }
        }

        public Issue CreateIssueFromCollection(Guid userId, NameValueCollection data)
        {
            Issue issue = new Issue();
            issue.PopulateWithData(data);
            issue.CreatedAt = issue.UpdatedAt;
            issue.CreatedByUserId = userId;

            // TODO: validate issue.

            if (!String.IsNullOrEmpty(issue.Title))
            {
                using (var db = DataService.Connect())
                {
                    db.Insert(issue);
                    issue.Id = db.GetLastInsertId();

                    var issueSearch = new FullTextSearchIssue()
                    {
                        DocId = issue.Id,
                        Text = issue.Text,
                        Title = issue.Title,
                    };
                    db.InsertParam(issueSearch);
                }
            }

            return issue;
        }

        public void UpdateIssueFromCollection(Guid userId, int issueId, NameValueCollection data)
        {
            Issue issue = new Issue();
            Dictionary<string, object> updates = issue.PopulateWithData(data);

            // TODO: validate issue.

            // TODO: create an IssueChange for each update key/value pair.
            //       create IssueComment from data.comment and IssueChange list

            using (var db = DataService.Connect())
            {
                db.UpdateOnly(issue, v => v.Update(updates.Keys.ToArray()).Where(i => i.Id == issueId));

                if (updates.ContainsKey("Text") || updates.ContainsKey("Title"))
                {
                    db.Update<FullTextSearchIssue>(new { Text = issue.Text, Title = issue.Title }, s => s.DocId == issueId);
                }
            }
        }

        //public static Issue PopulateIssueFromData(NameValueCollection data, List<string> updated)
        //{
        //    Issue issue = new Issue();

        //    foreach (string name in data.AllKeys)
        //    {
        //        string[] values = data.GetValues(name);
        //        string value = values[values.Length - 1];
        //        switch (name.ToLowerInvariant())
        //        {
        //            case "assigned":
        //            case "assignedto":
        //            case "assignedtouser":
        //                issue.AssignedToUserId = QueryService.GetUserFromName(value).Id;
        //                updated.Add("AssignedToUserId");
        //                break;

        //            case "assignedtoemail":
        //                issue.AssignedToUserId = QueryService.GetUserFromEmail(value).Id;
        //                updated.Add("AssignedToUserId");
        //                break;

        //            case "private":
        //                issue.Private = Convert.ToBoolean(value);
        //                updated.Add("Private");
        //                break;

        //            case "milestone":
        //            case "release":
        //                issue.Release = value;
        //                updated.Add("Release");
        //                break;

        //            case "resolution":
        //                issue.Resolution = value;
        //                updated.Add("Resolution");
        //                break;

        //            case "status":
        //                issue.Status = (IssueStatus)Enum.Parse(typeof(IssueStatus), value, true);
        //                updated.Add("Status");
        //                break;

        //            case "tag":
        //            case "tags":
        //                issue.Tags.Clear();
        //                issue.Tags.AddRange(values);
        //                updated.Add("Tags");
        //                break;

        //            case "content":
        //            case "text":
        //                issue.Text = value;
        //                updated.Add("Text");
        //                break;

        //            case "title":
        //                issue.Title = value;
        //                updated.Add("Title");
        //                break;

        //            case "type":
        //                issue.Type = (IssueType)Enum.Parse(typeof(IssueType), value, true);
        //                updated.Add("Type");
        //                break;

        //            case "vote":
        //            case "votes":
        //                issue.Votes = int.Parse(value);
        //                updated.Add("Votes");
        //                break;
        //        }
        //    }

        //    issue.UpdatedAt = DateTime.UtcNow;
        //    updated.Add("UpdatedAt");

        //    return issue;
        //}

        public void DeleteIssue(int issueId)
        {
            using (var db = DataService.Connect())
            {
                db.DeleteByIdParam<Issue>(issueId);
            }
        }
    }
}
