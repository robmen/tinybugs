namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Security;
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

            User user;
            if (q.Filters != null &&
                q.Filters.Any(f => f.Column.Equals("user", StringComparison.OrdinalIgnoreCase)) &&
                !UserService.TryAuthenticateUser(context.User, out user))
            {
                FormsAuthentication.RedirectToLoginPage(QueryService.RecreateQueryString(q));
                return null;
            }

            var issuesPaged = QueryService.QueryIssues(context.User, q);
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
