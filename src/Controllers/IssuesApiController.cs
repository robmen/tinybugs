namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

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

            return new JsonView(vm);
        }

        public override ViewBase Post(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            IssueViewModel vm = CreateIssueFromCollection(context, context.User, context.Form);
            if (vm == null)
            {
                return new StatusCodeView(HttpStatusCode.InternalServerError);
            }

            return new JsonView(vm, HttpStatusCode.Created);
        }

        public IssueViewModel CreateIssueFromCollection(ControllerContext context, Guid userId, NameValueCollection data)
        {
            IssueViewModel vm = null;
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

                    if (QueryService.TryGetIssueWithCommentsUsingDb(issue.Id, db, out vm))
                    {
                        vm.Location = context.ApplicationPath + vm.Id + "/";
                        var breadcrumbs = new BreadcrumbsViewModel(new Breadcrumb("Issues", context.ApplicationPath), new Breadcrumb("Issue #" + vm.Id + " - " + vm.Title, vm.Location));

                        FileService.WriteIssue(vm, breadcrumbs);
                        tx.Commit();
                    }
                }
            }

            return vm;
        }
    }
}
