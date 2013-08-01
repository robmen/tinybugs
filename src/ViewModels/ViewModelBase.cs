namespace RobMensching.TinyBugs.ViewModels
{
    using System.Net;

    public abstract class ViewModelBase
    {
        public ViewModelBase()
        {
            this.App = new AppViewModel();
            this.StatusCode = HttpStatusCode.OK;
        }

        public AppViewModel App { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
