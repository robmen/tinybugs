namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public class VerifyApiController : ControllerBase
    {
        public override void Execute()
        {
            string handlerPath = this.Context.ApplicationPath + "api/verify/";
#if DEBUG
            if (!this.Context.Url.AbsolutePath.WithTrailingSlash().StartsWithIgnoreCase(handlerPath))
            {
                throw new ApplicationException(String.Format("Paths are out of sync. Expected URL path to start with {0} but found {1}", handlerPath, this.Context.Url.AbsolutePath));
            }
#endif
            string token = this.Context.Url.AbsolutePath.WithTrailingSlash().Substring(handlerPath.Length).TrimEnd('/');

            DateTime issued;
            if (!UserService.TryValidateVerificationToken(token, out issued))
            {
                this.Context.SetStatusCode(HttpStatusCode.BadRequest);
            }
            else
            {
                using (var db = DataService.Connect())
                {
                    User user = db.FirstOrDefault<User>(u => u.VerifyToken == token);
                    if (user != null)
                    {
                        // Verification tokens are one shot deals. This one is dead now.
                        // Also, ensure the user is verifed now.
                        UserRole role = (user.Role == UserRole.Unverfied) ? UserRole.User : user.Role;
                        db.UpdateOnly(new User { VerifyToken = null, Role = role },
                            u => new { u.VerifyToken, u.Role },
                            u => u.Id == user.Id);
                    }
                    else
                    {
                        this.Context.SetStatusCode(HttpStatusCode.NotFound);
                    }
                }
            }
        }
    }
}
