namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("user")]
    [Route("user/{id}/{*username}")]
    public class UserController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            User me;
            if (!UserService.TryAuthenticateUser(context.User, out me))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }

            User user;
            long id;
            string idString = context.RouteData.Values["id"] as string;
            if (String.IsNullOrEmpty(idString))
            {
                user = me;
            }
            else if (!UserService.TryAuthorizeUser(me, UserRole.User))
            {
                return new StatusCodeView(HttpStatusCode.Unauthorized);
            }
            else if (!Int64.TryParse(idString, out id))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }
            else
            {
                using (var db = DataService.Connect(true))
                {
                    user = db.GetByIdOrDefault<User>(id);
                    if (user == null)
                    {
                        return new StatusCodeView(HttpStatusCode.NotFound);
                    }
                }
            }

            return new TemplateView("user.mustache", new
            {
                app = new AppViewModel(), 
                breadcrumbs = ConfigService.ExternalBreadcrumbs,
                user = new UserViewModel(user),
                me = new UserViewModel(me)
            });
        }
    }
}
