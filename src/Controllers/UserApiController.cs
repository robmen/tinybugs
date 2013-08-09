namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
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

            var user = QueryService.GetUserByName(Guid.Empty, username);
            if (user == null)
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            // TODO: create user view model and serialize that.
            //JsonSerializer.SerializeToWriter(vm, context.GetOutput("application/json"));
            return null;
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

            User user = QueryService.GetUserByName(Guid.Empty, username);
            if (user == null)
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
                db.UpdateOnly<User>(user, ev => ev.Update(u => u.Role).Where(u => u.Id == user.Id));
            }

            // TODO: create user view model and serialize that.
            //JsonSerializer.SerializeToWriter(vm, context.GetOutput("application/json"));
            return null;
        }

        public bool TryGetUserNameFromContext(ControllerContext context, out string username)
        {
            username = context.RouteData.Values["username"] as string;
            return username != null;
        }
    }
}
