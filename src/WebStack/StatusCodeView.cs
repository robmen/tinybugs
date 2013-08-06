namespace RobMensching.TinyWebStack
{
    using System.Net;
    using System.Web;

    public class StatusCodeView : ViewBase
    {
        public StatusCodeView(HttpStatusCode statusCode)
            : this((int)statusCode)
        {
        }

        public StatusCodeView(int statusCode)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; set; }

        public override void Execute(HttpContextBase context)
        {
            context.Response.StatusCode = this.StatusCode;
            context.Response.TrySkipIisCustomErrors = true;
        }
    }
}
