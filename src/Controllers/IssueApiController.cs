namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
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
            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            Issue issue;
            using (var db = DataService.Connect(true))
            {
                issue = db.GetByIdOrDefault<Issue>(issueId);
                if (issue == null)
                {
                    return new StatusCodeView(HttpStatusCode.NotFound);
                }
            }

            if (!user.IsInRole(UserRole.User))
            {
                return new StatusCodeView(HttpStatusCode.Forbidden);
            }

            bool unassigned = (issue.AssignedToUserId == 0);
            bool owner = (user.Id == issue.AssignedToUserId || user.Id == issue.CreatedByUserId);
            bool contributor =  user.IsInRole(UserRole.Contributor);

            PopulateResults results = issue.PopulateWithData(context.UnvalidatedForm, user.Guid);
            if (results.Errors.Count > 0)
            {
                return new JsonView(results.Errors, HttpStatusCode.BadRequest);
            }

            // Only a few fields can be updated by normal users.
            if (!owner && !contributor)
            {
                // AssignedTo may be changed if the issue isn't assigned to anyone. Status can be
                // changed but if it is changed we'll check it further next.
                if (results.Updates.Keys.Any(s => !((unassigned && s == "AssignedToUserId") || s == "Status" || s == "UpdatedAt")))
                {
                    return new StatusCodeView(HttpStatusCode.Forbidden);
                }
                else // if status is being changed, ensure it's being set to untriaged.
                {
                    PopulateResults.UpdatedValue statusChange;
                    if (results.Updates.TryGetValue("Status", out statusChange) &&
                        (IssueStatus)statusChange.New != IssueStatus.Untriaged)
                    {
                        return new StatusCodeView(HttpStatusCode.Forbidden);
                    }
                }
            }

            string comment = context.UnvalidatedForm.Get("comment");

            IssueViewModel vm = UpdateIssue(context, issue, user.Id, comment, results.Updates);
            if (vm == null)
            {
                return new StatusCodeView(HttpStatusCode.InternalServerError);
            }

            return new JsonView(vm);
        }

        public override ViewBase Delete(ControllerContext context)
        {
            long issueId;
            if (!this.TryGetIssueIdFromContext(context, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            Issue issue;
            using (var db = DataService.Connect(true))
            {
                issue = db.GetByIdOrDefault<Issue>(issueId);
                if (issue == null)
                {
                    return new StatusCodeView(HttpStatusCode.NotFound);
                }
            }

            if (user.Id != issue.CreatedByUserId &&
                !user.IsInRole(UserRole.Contributor))
            {
                return new StatusCodeView(HttpStatusCode.Forbidden);
            }

            this.DeleteIssue(issue.Id);
            return null;
        }

        public bool TryGetIssueIdFromContext(ControllerContext context, out long issueId)
        {
            string value = context.RouteData.Values["issue"] as string;
            return Int64.TryParse(value, out issueId);
        }

        public IssueViewModel UpdateIssue(ControllerContext context, Issue issue, long userId, string commentText, Dictionary<string, PopulateResults.UpdatedValue> updates)
        {
            IssueViewModel vm = null;

            IssueComment comment = new IssueComment();
            comment.IssueId = issue.Id;
            comment.CommentByUserId = userId;
            comment.CreatedAt = issue.UpdatedAt;
            comment.Text = commentText;
            foreach (var kvp in updates)
            {
                if (kvp.Key.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                object oldValue = kvp.Value.FriendlyOld ?? kvp.Value.Old ?? String.Empty;
                object newValue = kvp.Value.FriendlyNew ?? kvp.Value.New ?? String.Empty;

                IssueChange change = new IssueChange();
                change.Column = kvp.Value.FriendlyName ?? kvp.Key;
                change.Old = oldValue.ToString();
                change.New = newValue.ToString();
                comment.Changes.Add(change);
            }

            comment.Changes.Sort();

            using (var db = DataService.Connect())
            using (var tx = db.BeginTransaction())
            {
                // If there is some sort of recognizable change.
                if (!String.IsNullOrEmpty(comment.Text) || comment.Changes.Count > 0)
                {
                    db.UpdateOnly(issue, v => v.Update(updates.Keys.ToArray()).Where(i => i.Id == issue.Id));

                    if (updates.ContainsKey("Text") || updates.ContainsKey("Title"))
                    {
                        db.Update<FullTextSearchIssue>(new { Text = issue.Text, Title = issue.Title }, s => s.DocId == issue.Id);
                    }

                    db.Insert(comment);
                }

                if (QueryService.TryGetIssueWithCommentsUsingDb(issue.Id, db, out vm))
                {
                    vm.Location = context.ApplicationPath + vm.Id + "/";
                    var breadcrumbs = new BreadcrumbsViewModel(new Breadcrumb("Issues", context.ApplicationPath), new Breadcrumb("#" + vm.Id + " - " + vm.Title, vm.Location));

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
