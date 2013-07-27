namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Web;
    using ServiceStack.Text;

    public class ControllerContext
    {
        public ControllerContext(HttpContext context)
        {
            this.Context = context;
        }

        public HttpContext Context { get; private set; }

        public string ApplicationPath { get { return this.Context.Request.ApplicationPath.WithTrailingSlash(); } }

        public string Method { get { return this.Context.Request.HttpMethod; } }

        public bool Authenticated { get { return (this.Context.User != null && this.Context.User.Identity.IsAuthenticated); } }

        public HttpCookieCollection RequestCookies { get { return this.Context.Request.Cookies; } }

        public HttpCookieCollection Cookies { get { return this.Context.Response.Cookies; } }

        public NameValueCollection QueryString { get { return this.Context.Request.QueryString; } }

        public NameValueCollection Form { get { return this.Context.Request.Form; } }

        public Uri Referrer { get { return this.Context.Request.UrlReferrer; } }

        public Uri Url { get { return this.Context.Request.Url; } }

        public Guid User { get { return (this.Context.User != null && this.Context.User.Identity.IsAuthenticated) ? new Guid(this.Context.User.Identity.Name) : Guid.Empty; } }

        public TextWriter GetOutput(string contentType = null)
        {
            this.Context.Response.ContentType = contentType;
            return this.Context.Response.Output;
        }

        public void Redirect(string url, bool permanent = false)
        {
            if (permanent)
            {
                this.Context.Response.RedirectPermanent(url, false);
            }
            else
            {
                this.Context.Response.Redirect(url, false);
            }

            this.Context.ApplicationInstance.CompleteRequest();
        }

        public void SetStatusCode(HttpStatusCode statusCode)
        {
            this.SetStatusCode((int)statusCode);
        }

        public void SetStatusCode(int statusCode)
        {
            this.Context.Response.StatusCode = statusCode;
            this.Context.Response.TrySkipIisCustomErrors = true;
        }
    }
}
