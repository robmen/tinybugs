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
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            var user = QueryService.GetUserFromName(username);
            if (user == null)
            {
                context.SetStatusCode(HttpStatusCode.NotFound);
                return null;
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
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            if (!context.Authenticated)
            {
                context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
                return null;
            }

            using (var db = DataService.Connect(true))
            {
                User me = db.GetById<User>(context.User);
                if (me.Role < UserRole.Admin)
                {
                    context.SetStatusCode(HttpStatusCode.Forbidden);
                    return null;
                }
            }

            User user = QueryService.GetUserFromName(username);
            if (user == null)
            {
                context.SetStatusCode(HttpStatusCode.NotFound);
                return null;
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
