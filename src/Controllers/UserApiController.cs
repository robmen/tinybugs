namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("api/user/{username}")]
    public class UserApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            string username;
            if (!this.TryGetUserNameFromContext(context, out username))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            User me;
            if (!UserService.TryAuthenticateUser(context.User, out me))
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            if (username.Equals("0"))
            {
                username = "[me]";
            }

            User user;
            if (!QueryService.TryGetUserByName(me.Guid, username, out user))
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            return new JsonView(new UserViewModel(user));
        }

        public override ViewBase Post(ControllerContext context)
        {
            // Forward POST to PUT for those clients that only use POST.
            return this.Put(context);
        }

        public override ViewBase Put(ControllerContext context)
        {
            string username;
            if (!this.TryGetUserNameFromContext(context, out username))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            User me;
            if (!UserService.TryAuthenticateUser(context.User, out me))
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            if (!UserService.TryAuthorizeUser(me, UserRole.Admin))
            {
                return new StatusCodeView(HttpStatusCode.Forbidden);
            }

            User user;
            if (!QueryService.TryGetUserByName(context.User, username, out user))
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            UserRole role;
            string newRole = context.Form["role"];
            if (!Enum.TryParse<UserRole>(newRole, true, out role))
            {
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            user.Role = role;
            using (var db = DataService.Connect())
            {
                db.UpdateOnly<User>(user, ev => ev.Update(u => u.Role).Where(u => u.Guid == user.Guid));
            }

            return new JsonView(new UserViewModel(user));
        }

        public bool TryGetUserNameFromContext(ControllerContext context, out string username)
        {
            username = context.RouteData.Values["username"] as string;
            return username != null;
        }
    }
}
