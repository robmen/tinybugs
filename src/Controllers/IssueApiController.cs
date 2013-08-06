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
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            IssueViewModel issue;
            if (!QueryService.TryGetIssueWithComments(issueId, out issue))
            {
                context.SetStatusCode(HttpStatusCode.NotFound);
                return null;
            }

            issue.Location = context.ApplicationPath + issue.Id + "/";
            JsonSerializer.SerializeToWriter(issue, context.GetOutput("application/json"));
            return null;
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
                context.SetStatusCode(HttpStatusCode.Unauthorized);
                return null;
            }

            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            IssueViewModel ci = UpdateIssueFromCollection(context.User, issueId, context.Form);
            if (ci == null)
            {
                context.SetStatusCode(HttpStatusCode.InternalServerError);
            }
            else
            {
                ci.Location = context.ApplicationPath + ci.Id + "/";
            }

            JsonSerializer.SerializeToWriter(ci, context.GetOutput("application/json"));
            return null;
        }

        public override ViewBase Delete(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                context.SetStatusCode(HttpStatusCode.Unauthorized);
                return null;
            }

            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            this.DeleteIssue(issueId);
            return null;
        }

        public bool TryGetIssueIdFromContext(ControllerContext context, out long issueId)
        {
            string value = context.RouteData.Values["issue"] as string;
            return Int64.TryParse(value, out issueId);
        }

        public IssueViewModel UpdateIssueFromCollection(Guid userId, long issueId, NameValueCollection data)
        {
            IssueViewModel ci = null;
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

                if (QueryService.TryGetIssueWithCommentsUsingDb(issueId, db, out ci))
                {
                    FileService.WriteIssue(ci);
                    tx.Commit();
                }
            }

            return ci;
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
