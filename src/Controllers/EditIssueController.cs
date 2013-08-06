namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using Nustache.Core;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;

    [Route("edit/{issue}")]
    public class EditIssueController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            string value = context.RouteData.Values["issue"] as string;
            long issueId = Int64.Parse(value);

            IssueViewModel issue;
            if (QueryService.TryGetIssueWithComments(issueId, out issue))
            {
                Template template = FileService.LoadTemplate("bugform.mustache");
                template.Render(new { app = new AppViewModel(), issue = issue }, context.GetOutput(), null);
            }
            else
            {
                context.SetStatusCode(HttpStatusCode.NotFound);
            }

            return null;
        }
    }
}
