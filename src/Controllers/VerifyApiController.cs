namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("api/verify")]
    [Route("api/verify/{token}")]
    public class VerifyApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            string token = context.RouteData.Values["token"] as string;
            if (String.IsNullOrEmpty(token))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            return VerifyToken(context, token);
        }

        public override ViewBase Post(ControllerContext context)
        {
            string token = context.RouteData.Values["token"] as string;
            if (String.IsNullOrEmpty(token))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            string password = context.Form["password"] ?? String.Empty;
            string confirm = context.Form["verifypassword"];
            if (String.IsNullOrEmpty(password) && password.Equals(confirm))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            return VerifyToken(context, token, password);
        }

        public ViewBase VerifyToken(ControllerContext context, string token, string password = null)
        {
            if (!token.StartsWith("t"))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            token = token.Substring(1); // remove the expected "t" from the beginning of the token.

            DateTime issued;
            if (!UserService.TryValidateVerificationToken(token, out issued))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            using (var db = DataService.Connect())
            {
                User user = db.FirstOrDefault<User>(u => u.VerifyToken == token);
                if (user == null)
                {
                    return new StatusCodeView(HttpStatusCode.NotFound);
                }

                string passwordHash = user.PasswordHash;
                if (!String.IsNullOrEmpty(password))
                {
                    passwordHash = UserService.CalculatePasswordHash(user.Guid, user.Salt, password);
                }

                // Verification tokens are one shot deals. This one is dead now.
                // Also, ensure the user is verifed now.
                UserRole role = (user.Role == UserRole.Unverfied) ? UserRole.User : user.Role;
                db.UpdateOnly(new User { VerifyToken = null, Role = role, PasswordHash = passwordHash },
                    u => new { u.VerifyToken, u.Role, u.PasswordHash },
                    u => u.Guid == user.Guid);
            }

            return null;
        }
    }
}
