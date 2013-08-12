namespace RobMensching.TinyBugs.Controllers
{
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;

    [Route("")]
    [Route("search")]
    public class SearchController : ControllerBase
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

            string path = q.Template ?? "search.mustache";
            return new TemplateView(path, vm);
        }
    }
}
