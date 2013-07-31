namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using Nustache.Core;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.Text;

    public class EditIssueController : ControllerBase
    {
        public override void Execute()
        {
            string handlerPath = this.Context.ApplicationPath + "edit/";
            string path = this.Context.Url.AbsolutePath.WithTrailingSlash().Substring(handlerPath.Length).TrimEnd('/');
            Query q = QueryService.ParseQuery(this.Context.QueryString);

            long issueId = 0;
            if (!long.TryParse(path, out issueId))
            {
                path = null;
            }

            var app = new AppViewModel(this.Context.ApplicationPath);
            CompleteIssue issue;
            if (!QueryService.TryGetIssueWithComments(issueId, out issue))
            {
                this.Context.SetStatusCode(HttpStatusCode.NotFound);
            }
            else
            {
                Template template = FileService.LoadTemplate("bugform.mustache");
                template.Render(new { app = app, issue = issue }, this.Context.GetOutput(), null);
            }
        }
    }
}
