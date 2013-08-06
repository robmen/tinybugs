namespace RobMensching.TinyWebStack
{
    using System.Web;

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
            if (this.Permanent)
            {
                context.Response.RedirectPermanent(this.Url, false);
            }
            else
            {
                context.Response.Redirect(this.Url, false);
            }

            context.ApplicationInstance.CompleteRequest();
        }
    }
}
