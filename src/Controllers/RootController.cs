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
                Page = new PaginationViewModel(q.Page, q.Count, issuesPaged.Total, pagePrefix),
            };

            string path = q.Template ?? "root.mustache";
            var template = FileService.LoadTemplate(path);
            template.Render(vm, this.Context.GetOutput(), null);
        }
    }
}
