namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    [Route("api/issue")]
    public class IssuesApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            Query q = QueryService.ParseQuery(context.QueryString);
            QueriedIssuesViewModel issues = QueryService.QueryIssues(q);
            issues.Issues.ForEach(i => i.Location = context.ApplicationPath + i.Id + "/"); // TODO: come up with a better way of handling this.
            var pagePrefix = context.ControllerPath + QueryService.RecreateQueryString(q) + "&page=";

            IssuesApiViewModel vm = new IssuesApiViewModel()
            {
                Issues = issues.Issues,
                Page = new PaginationViewModel(q.Page, q.Count, issues.Total, pagePrefix),
            };

            JsonSerializer.SerializeToWriter(vm, context.GetOutput("application/json"));
            return null;
        }

        public override ViewBase Post(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
                return null;
            }

            IssueViewModel ci = CreateIssueFromCollection(context.User, context.Form);
            if (ci == null)
            {
                context.SetStatusCode(HttpStatusCode.InternalServerError);
                return null;
            }

            context.SetStatusCode(HttpStatusCode.Created);
            ci.Location = context.ApplicationPath + ci.Id + "/";
            JsonSerializer.SerializeToWriter(ci, context.GetOutput("application/json"));

            return null;
        }

        public IssueViewModel CreateIssueFromCollection(Guid userId, NameValueCollection data)
        {
            IssueViewModel ci = null;
            Issue issue = new Issue();
            issue.PopulateWithData(data, userId);
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
    }
}
