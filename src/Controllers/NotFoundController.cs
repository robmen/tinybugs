namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyBugs.Views;
    using RobMensching.TinyWebStack;
    using ServiceStack.Logging;

    [Route("notfound")]
    public class NotFoundController : ControllerBase
    {
        private static ILog Log = LogManager.GetLogger("notfound");

        public override ViewBase Get(ControllerContext context)
        {
            var query = context.QueryString;
            HttpStatusCode statusCode = HttpStatusCode.NotFound;
            string referrer = context.Referrer != null ? context.Referrer.AbsoluteUri : String.Empty;

            if (query.Count > 0)
            {
                string[] s = query.Get(0).Split(new char[] { ';' });
                statusCode = (HttpStatusCode)Convert.ToInt32(s[0]);
                if (String.IsNullOrEmpty(referrer) && s.Length > 1)
                {
                    referrer = s[1];
                }
            }

            Log.InfoFormat("url: {0}  referrer: {1}", context.Url.AbsoluteUri, referrer);

            return new TemplateView("notfound.mustache", new { ReferralUri = referrer, StatusCode = statusCode }, statusCode);
        }
    }
}
