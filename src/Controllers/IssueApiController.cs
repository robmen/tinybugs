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

    [Route("api/issue/{issue}")]
    public class IssueApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            IssueViewModel issue;
            if (!QueryService.TryGetIssueWithComments(issueId, out issue))
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            issue.Location = context.ApplicationPath + issue.Id + "/";
            return new JsonView(issue);
        }

        public override ViewBase Post(ControllerContext context)
        {
            // Forward POST to PUT for those clients that only use POST.
            return this.Put(context);
        }

        public override ViewBase Put(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            IssueViewModel ci = UpdateIssueFromCollection(context, context.User, issueId, context.UnvalidatedForm);
            if (ci == null)
            {
                context.SetStatusCode(HttpStatusCode.InternalServerError);
            }

            return new JsonView(ci);
        }

        public override ViewBase Delete(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            this.DeleteIssue(issueId);
            return null;
        }

        public bool TryGetIssueIdFromContext(ControllerContext context, out long issueId)
        {
            string value = context.RouteData.Values["issue"] as string;
            return Int64.TryParse(value, out issueId);
        }

        public IssueViewModel UpdateIssueFromCollection(ControllerContext context, Guid userId, long issueId, NameValueCollection data)
        {
            IssueViewModel vm = null;
            Issue issue = new Issue();
            Dictionary<string, object> updates = issue.PopulateWithData(data, userId);

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

                if (QueryService.TryGetIssueWithCommentsUsingDb(issueId, db, out vm))
                {
                    vm.Location = context.ApplicationPath + vm.Id + "/";
                    var breadcrumbs = new BreadcrumbsViewModel(new Breadcrumb("Issues", context.ApplicationPath), new Breadcrumb("Issue #" + vm.Id + " - " + vm.Title, vm.Location));

                    FileService.WriteIssue(vm, breadcrumbs);
                    tx.Commit();
                }
            }

            return vm;
        }

        public void DeleteIssue(long issueId)
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
