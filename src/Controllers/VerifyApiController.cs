namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("api/verify/{token}")]
    public class VerifyApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            string token;
            if (!this.TryGetTokenFromContext(context, out token))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

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

                // Verification tokens are one shot deals. This one is dead now.
                // Also, ensure the user is verifed now.
                UserRole role = (user.Role == UserRole.Unverfied) ? UserRole.User : user.Role;
                db.UpdateOnly(new User { VerifyToken = null, Role = role },
                    u => new { u.VerifyToken, u.Role },
                    u => u.Id == user.Id);
            }

            return null;
        }

        public bool TryGetTokenFromContext(ControllerContext context, out string token)
        {
            token = context.RouteData.Values["token"] as string;
            return token != null;
        }
    }
}
