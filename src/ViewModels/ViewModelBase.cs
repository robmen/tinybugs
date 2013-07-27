namespace RobMensching.TinyBugs.ViewModels
{
    using System.Net;

    public abstract class ViewModelBase
    {
        public ViewModelBase()
        {
            this.StatusCode = HttpStatusCode.OK;
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
