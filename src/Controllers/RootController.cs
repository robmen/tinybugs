namespace RobMensching.TinyBugs.Controllers
{
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;

    public class RootController : ControllerBase
    {
        public override void Execute()
        {
            Query q = QueryService.ParseQuery(this.Context.QueryString);
            var issuesPaged = QueryService.QueryIssues(q);
            var pagePrefix = this.Context.ApplicationPath + QueryService.RecreateQueryString(q) + "&page=";

            RootViewModel vm = new RootViewModel()
            {
                Issues = issuesPaged.Issues,
                Page = issuesPaged.Page,
                Pages = issuesPaged.Pages,
                Total = issuesPaged.Total,
                PageUriPrefix = pagePrefix,
                PreviousPageUri = (issuesPaged.PreviousPage > 0) ? pagePrefix + issuesPaged.PreviousPage : null,
                NextPageUri = (issuesPaged.NextPage > 0) ? pagePrefix + issuesPaged.NextPage : null,
            };

            string path = q.Template ?? "root.html.mustache";
            var template = FileService.LoadTemplate(path);
            template.Render(vm, this.Context.GetOutput(), null);
        }
    }
}
