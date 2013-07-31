namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Web;
    using System.Web.Security;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;

    public class SessionApiController : ControllerBase
    {
        public override void Execute()
        {
            LoginViewModel vm;

            switch (this.Context.Method)
            {
                case "GET":
                    // TODO: verify whether the user is logged in.
                    vm = new LoginViewModel() { StatusCode = System.Net.HttpStatusCode.OK };
                    break;

                case "POST":
                    vm = this.Login();
                    vm.Retry = true;
                    break;

                case "DELETE":
                    vm = this.Logout();
                    break;

                default:
                    vm = new LoginViewModel() { StatusCode = System.Net.HttpStatusCode.MethodNotAllowed };
                    break;
            }

            if (!String.IsNullOrEmpty(vm.RedirectUrl))
            {
                this.Context.Redirect(vm.RedirectUrl);
            }
            else
            {
                this.Context.SetStatusCode(vm.StatusCode);
                if (vm.Template != null)
                {
                    vm.Template.Render(vm, this.Context.GetOutput(), null);
                }
            }
        }

        public LoginViewModel Login()
        {
            LoginViewModel vm = new LoginViewModel();

            string username = this.Context.Form["username"];
            string password = this.Context.Form["password"];

            User user;
            if (UserService.TryAuthenticateUser(username, password, out user))
            {
                string userId = user.Id.ToString();

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(userId, false, 30);
                this.Context.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));

                vm.GravatarId = user.GravatarId;
                vm.UserEmail = user.Email;
                vm.UserName = user.UserName;
                vm.UserFullName = user.FullName;

                // Forms authentication is a bit old fashioned and will automatically add "default.aspx" on to
                // URLs lacking a filename. Remove that default document and let the web server find the default
                // document itself.
                vm.RedirectUrl = FormsAuthentication.GetRedirectUrl(userId, false).ToLowerInvariant().Replace("default.aspx", String.Empty);
            }
            else
            {
                vm.StatusCode = System.Net.HttpStatusCode.BadGateway; // TODO: return a better error code that doesn't cause forms authentication to overwrite our response.
                vm.UserName = username;
            }

            return vm;
        }

        public LoginViewModel Logout()
        {
            FormsAuthentication.SignOut();
            return new LoginViewModel() { RedirectUrl = "/" };
        }
    }
}
