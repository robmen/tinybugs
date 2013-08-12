namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;

    [Route("edit/{issue}")]
    public class EditIssueController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            long issueId;
            if (!Int64.TryParse(context.RouteData.Values["issue"] as string, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }

            IssueViewModel issue;
            if (!QueryService.TryGetIssueWithComments(issueId, out issue))
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            if (!user.Email.Equals(issue.AssignedToUserEmail) &&
                !user.Email.Equals(issue.CreatedByUserEmail) &&
                !user.IsInRole(UserRole.Contributor))
            {
                return new RedirectView("~/accessdenied/");
            }

            return new TemplateView("bugform.mustache", new { app = new AppViewModel(), issue = issue });
        }
    }
}
