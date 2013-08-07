namespace RobMensching.TinyBugs.Controllers
{
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;

    [Route("")]
    public class RootController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            Query q = QueryService.ParseQuery(context.QueryString);
            var issuesPaged = QueryService.QueryIssues(q);
            var pagePrefix = context.ControllerPath + QueryService.RecreateQueryString(q) + "&page=";

            RootViewModel vm = new RootViewModel()
            {
                Breadcrumbs = new BreadcrumbsViewModel(new Breadcrumb("Issues", context.ControllerPath)),
                Issues = issuesPaged.Issues,
                Page = new PaginationViewModel(q.Page, q.Count, issuesPaged.Total, pagePrefix),
            };

            string path = q.Template ?? "root.mustache";
            var template = FileService.LoadTemplate(path);
            template.Render(vm, context.GetOutput(), null);
            return null;
        }
    }
}
