namespace RobMensching.TinyWebStack
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Web.Routing;
    using ServiceStack.Text;

    public class ControllerContext
    {
        public ControllerContext(RequestContext context)
        {
            this.Context = context.HttpContext;
            this.RouteData = context.RouteData;
        }

        public HttpContextBase Context { get; private set; }

        public RouteData RouteData { get; private set; }

        public string ApplicationPath { get { return this.Context.Request.ApplicationPath.WithTrailingSlash(); } }

        public string ControllerPath { get { return this.Context.Request.Url.AbsolutePath; } }

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
