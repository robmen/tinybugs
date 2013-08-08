namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using Nustache.Core;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;

    [Route("edit/{issue}")]
    public class EditIssueController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }
            else if (!UserService.TryAuthorizeUser(user, UserRole.Contributor))
            {
                return new RedirectView("~/accessdenied/");
            }

            long issueId;
            if (!Int64.TryParse(context.RouteData.Values["issue"] as string, out issueId))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            IssueViewModel issue;
            if (!QueryService.TryGetIssueWithComments(issueId, out issue))
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            Template template = FileService.LoadTemplate("bugform.mustache");
            FileService.RenderTemplateToWriter(template, new { app = new AppViewModel(), issue = issue }, context.GetOutput());
            return null;
        }
    }
}
