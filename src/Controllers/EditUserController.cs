namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;

    [Route("edit/user")]
    public class EditUserController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            User user;
            if (!UserService.TryAuthenticateUser(context.User, out user))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }

            return new TemplateView("userform.mustache", new { app = new AppViewModel(), user = new UserViewModel(user), });
        }
    }
}
