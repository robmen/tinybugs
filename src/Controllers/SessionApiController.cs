namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Security;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;

    [Route("api/session")]
    public class SessionApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            return null;
        }

        public override ViewBase Post(ControllerContext context)
        {
            ViewBase view;
            string username = context.Form["username"];
            string password = context.Form["password"];

            User user;
            if (UserService.TryAuthenticateUser(username, password, out user))
            {
                string userId = user.Guid.ToString();

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(userId, false, 30);
                context.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));

                // Forms authentication is a bit old fashioned and will automatically add "default.aspx" on to
                // URLs lacking a filename. Remove that default document and let the web server find the default
                // document itself.
                string redirect = FormsAuthentication.GetRedirectUrl(userId, false).ToLowerInvariant().Replace("default.aspx", String.Empty);
                view = new RedirectView(redirect);
            }
            else
            {
                view = new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }

            return view;
        }

        public override ViewBase Delete(ControllerContext context)
        {
            FormsAuthentication.SignOut();
            return new RedirectView(context.ApplicationPath);
        }
    }
}
