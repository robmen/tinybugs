namespace RobMensching.TinyBugs.ViewModels
{
    using Nustache.Core;

    public class LoginViewModel : ViewModelBase
    {
        public string RedirectUrl { get; set; }

        public bool Retry { get; set; }

        public Template Template { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }
    }
}
