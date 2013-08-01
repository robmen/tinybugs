namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public class IssuesApiController : ControllerBase
    {
        public override void Execute()
        {
            string handlerPath = this.Context.ApplicationPath + "api/issue/";
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
                            QueriedIssuesViewModel issues = QueryService.QueryIssues(q);
                            issues.Issues.ForEach(i => i.Location = this.Context.ApplicationPath + i.Id + "/"); // TODO: come up with a better way of handling this.
                            var pagePrefix = handlerPath + QueryService.RecreateQueryString(q) + "&page=";

                            IssuesApiViewModel vm = new IssuesApiViewModel()
                            {
                                Issues = issues.Issues,
                                Page = new PaginationViewModel(q.Page, q.Count, issues.Total, pagePrefix),
                            };
                            JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                        }
                        break;

                    case "POST":
                        {
                            if (!this.Context.Authenticated)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
                            }
                            else
                            {
                                IssueViewModel ci = CreateIssueFromCollection(this.Context.User, this.Context.Form);
                                if (ci == null)
                                {
                                    this.Context.SetStatusCode(HttpStatusCode.InternalServerError);
                                }
                                else
                                {
                                    ci.Location = this.Context.ApplicationPath + ci.Id + "/";

                                    this.Context.SetStatusCode(HttpStatusCode.Created);
                                    JsonSerializer.SerializeToWriter(ci, this.Context.GetOutput("application/json"));
                                }
                            }
                        }
                        break;

                    default:
                        this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                        break;
                }
            }
            else // interact with existing issue.
            {
                IssueViewModel issue;
                if (!QueryService.TryGetIssueWithComments(issueId, out issue))
                {
                    this.Context.SetStatusCode(HttpStatusCode.NotFound);
                }
                else
                {
                    switch (this.Context.Method)
                    {
                        case "GET":
                            issue.Location = this.Context.ApplicationPath + issue.Id + "/";
                            break;

                        case "PUT":
                        case "POST":
                            if (!this.Context.Authenticated)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.Unauthorized);
                            }
                            else
                            {
                                IssueViewModel ci = UpdateIssueFromCollection(this.Context.User, issue.Id, this.Context.Form);
                                if (ci == null)
                                {
                                    this.Context.SetStatusCode(HttpStatusCode.InternalServerError);
                                }
                                else
                                {
                                    issue.Location = this.Context.ApplicationPath + issue.Id + "/";
                                }
                            }
                            break;

                        case "DELETE":
                            if (!this.Context.Authenticated)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.Unauthorized);
                            }
                            else
                            {
                                DeleteIssue(issue.Id);
                                this.Context.SetStatusCode(HttpStatusCode.OK);
                                issue = null;
                            }
                            break;

                        default:
                            this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                            issue = null;
                            break;
                    }

                    if (issue != null)
                    {
                        JsonSerializer.SerializeToWriter(issue, this.Context.GetOutput("application/json"));
                    }
                }
            }
        }

        public IssueViewModel CreateIssueFromCollection(Guid userId, NameValueCollection data)
        {
            IssueViewModel ci = null;
            Issue issue = new Issue();
            issue.PopulateWithData(data);
            issue.CreatedAt = issue.UpdatedAt;
            issue.CreatedByUserId = userId;

            // TODO: validate issue.

            if (!String.IsNullOrEmpty(issue.Title))
            {
                using (var db = DataService.Connect())
                using (var tx = db.BeginTransaction())
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

                    if (QueryService.TryGetIssueWithCommentsUsingDb(issue.Id, db, out ci))
                    {
                        FileService.WriteIssue(ci);
                        tx.Commit();
                    }
                }
            }

            return ci;
        }

        public IssueViewModel UpdateIssueFromCollection(Guid userId, int issueId, NameValueCollection data)
        {
            IssueViewModel ci = null;
            Issue issue = new Issue();
            Dictionary<string, object> updates = issue.PopulateWithData(data);

            // TODO: validate issue.

            // TODO: create an IssueChange for each update key/value pair.
            //       create IssueComment from data.comment and IssueChange list

            using (var db = DataService.Connect())
            using (var tx = db.BeginTransaction())
            {
                db.UpdateOnly(issue, v => v.Update(updates.Keys.ToArray()).Where(i => i.Id == issueId));

                if (updates.ContainsKey("Text") || updates.ContainsKey("Title"))
                {
                    db.Update<FullTextSearchIssue>(new { Text = issue.Text, Title = issue.Title }, s => s.DocId == issueId);
                }

                if (QueryService.TryGetIssueWithCommentsUsingDb(issueId, db, out ci))
                {
                    FileService.WriteIssue(ci);
                    tx.Commit();
                }
            }

            return ci;
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
            using (var tx = db.BeginTransaction())
            {
                db.DeleteByIdParam<Issue>(issueId);
                FileService.RemoveIssue(issueId);
                tx.Commit();
            }

        }
    }
}
