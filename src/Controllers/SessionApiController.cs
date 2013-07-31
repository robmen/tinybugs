namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Security;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;

    public class SessionApiController : ControllerBase
    {
        public override void Execute()
        {
            string redirect = null;
            switch (this.Context.Method)
            {
                case "GET":
                    if (!this.Context.Authenticated)
                    {
                        this.Context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
                    }
                    break;

                case "POST":
                    redirect = this.Login();
                    break;

                case "DELETE":
                    redirect = this.Logout();
                    break;

                default:
                    this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                    break;
            }

            if (!String.IsNullOrEmpty(redirect))
            {
                this.Context.Redirect(redirect);
            }
        }

        public string Login()
        {
            string redirect = null;
            string username = this.Context.Form["username"];
            string password = this.Context.Form["password"];

            User user;
            if (UserService.TryAuthenticateUser(username, password, out user))
            {
                string userId = user.Id.ToString();

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(userId, false, 30);
                this.Context.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));

                // Forms authentication is a bit old fashioned and will automatically add "default.aspx" on to
                // URLs lacking a filename. Remove that default document and let the web server find the default
                // document itself.
                redirect = FormsAuthentication.GetRedirectUrl(userId, false).ToLowerInvariant().Replace("default.aspx", String.Empty);
            }
            else
            {
                this.Context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response.
            }

            return redirect;
        }

        public string Logout()
        {
            FormsAuthentication.SignOut();
            return this.Context.ApplicationPath;
        }
    }
}
