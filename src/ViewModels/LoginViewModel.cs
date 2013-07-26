namespace RobMensching.TinyBugs.ViewModels
{
    using System.Net;
    using Nustache.Core;

    public class LoginViewModel
    {
        public LoginViewModel()
        {
            this.StatusCode = HttpStatusCode.OK;
        }

        public HttpStatusCode StatusCode { get; set; }

        public string RedirectUrl { get; set; }

        public bool Retry { get; set; }

        public Template Template { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }
    }
}
