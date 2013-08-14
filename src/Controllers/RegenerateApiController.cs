namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using System.Web.Routing;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("api/regenerate")]
    public class RegenerateApiController : ControllerBase
    {
        public override ViewBase Post(ControllerContext context)
        {
            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }
            else if (!UserService.TryAuthorizeUser(user, UserRole.Admin))
            {
                return new StatusCodeView(HttpStatusCode.Forbidden);
            }

            using (var db = DataService.Connect(true))
            {
                FileService.PregenerateApp();

                var issuesIds = db.SqlList<long>("SELECT Id FROM Issue");
                foreach (var issueId in issuesIds)
                {
                    IssueViewModel vm;
                    if (QueryService.TryGetIssueWithCommentsUsingDb(issueId, db, out vm))
                    {
                        vm.Location = context.ApplicationPath + vm.Id + "/";
                        var breadcrumbs = new BreadcrumbsViewModel(new Breadcrumb("Issues", context.ApplicationPath), new Breadcrumb("#" + vm.Id + " - " + vm.Title, vm.Location));

                        FileService.WriteIssue(vm, breadcrumbs);
                    }
                }
            }

            return null;
        }
    }
}
