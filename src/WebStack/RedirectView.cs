namespace RobMensching.TinyWebStack
{
    using System.Web;
    using ServiceStack.Text;

    public class RedirectView : ViewBase
    {
        public RedirectView(string url, bool permanent = false)
        {
            this.Url = url;
            this.Permanent = permanent;
        }

        public bool Permanent { get; set; }

        public string Url { get; set; }

        public override void Execute(HttpContextBase context)
        {
            string url = this.Url.StartsWith("~/") ? context.Request.ApplicationPath.WithTrailingSlash() + this.Url.Substring(2) : this.Url;

            if (this.Permanent)
            {
                context.Response.RedirectPermanent(url, false);
            }
            else
            {
                context.Response.Redirect(url, false);
            }

            context.ApplicationInstance.CompleteRequest();
        }
    }
}
