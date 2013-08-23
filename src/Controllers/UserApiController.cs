namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

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
            else if (username.Equals("0"))
            {
                username = "[me]";
            }

            User me;
            if (!UserService.TryAuthenticateUser(context.User, out me))
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            User user;
            if (!QueryService.TryGetUserByName(context.User, username, out user))
            {
                return new StatusCodeView(HttpStatusCode.NotFound);
            }

            var results = user.PopulateWithData(context.Form, me);
            if (results.Errors.Count == 0)
            {
                using (var db = DataService.Connect())
                using (var tx = db.BeginTransaction())
                {
                    db.UpdateOnly(user, v => v.Update(results.Updates.Keys.ToArray()).Where(u => u.Id == user.Id));

                    if (results.Updates.Any(u => u.Key.Equals("VerifyToken")))
                    {
                        MailService.SendVerifyUser(user.Email, user.VerifyToken);
                    }

                    tx.Commit();
                }
            }

            return new JsonView(new { user = new UserViewModel(user), errors = results.Errors },
                                results.Errors.Count == 0 ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        }

        public bool TryGetUserNameFromContext(ControllerContext context, out string username)
        {
            username = context.RouteData.Values["username"] as string;
            return username != null;
        }
    }
}
