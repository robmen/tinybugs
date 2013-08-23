namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;

    [Route("edit/me")]
    public class EditUserController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            User me;
            if (!UserService.TryAuthenticateUser(context.User, out me))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }

            return new TemplateView("userform.mustache", new { app = new AppViewModel(), user = new UserViewModel(me), });
        }
    }
}
