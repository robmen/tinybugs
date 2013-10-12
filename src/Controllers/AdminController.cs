namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;

    [Route("admin")]
    public class AdminController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }

            if (!user.IsInRole(UserRole.Admin))
            {
                return new RedirectView("~/accessdenied/");
            }

            return new TemplateView("admin.mustache", new { app = new AppViewModel(), breadcrumbs = ConfigService.ExternalBreadcrumbs, mail = ConfigService.Mail });
        }
    }
}
