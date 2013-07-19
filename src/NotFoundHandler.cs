namespace RobMensching.TinyBugs
{
    using System;
    using System.IO;
    using System.Web;
    using NLog;
    using Nustache.Core;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;

    public class NotFoundHandler : IHttpHandler
    {
        private static Logger Log = LogManager.GetLogger("notfound");

        static NotFoundHandler()
        {
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string referrer = context.Request.UrlReferrer != null ? context.Request.UrlReferrer.AbsoluteUri : String.Empty;
            int statusCode = 404;

            if (String.IsNullOrEmpty(referrer))
            {
                string[] s = context.Request.Url.Query.Substring(1).Split(new char[] { ';' });
                statusCode = Convert.ToInt32(s[0]);
                referrer = s[1];
            }

            Log.Info("url: {0}  referrer: {1}", context.Request.Url.AbsoluteUri, referrer);

            var viewModel = new NotFoundViewModel() { ReferralUri = referrer, StatusCode = statusCode };
            var template = FileService.LoadTemplate("notfound/index.mustache");

            context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
            template.Render(viewModel, context.Response.Output, null);
        }
    }
}
