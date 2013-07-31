namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.Logging;

    public class NotFoundController : ControllerBase
    {
        private static ILog Log = LogManager.GetLogger("notfound");

        public override void Execute()
        {
            var query = this.Context.QueryString;
            HttpStatusCode statusCode = HttpStatusCode.NotFound;
            string referrer = this.Context.Referrer != null ? this.Context.Referrer.AbsoluteUri : String.Empty;

            if (query.Count > 0)
            {
                string[] s = query.Get(0).Split(new char[] { ';' });
                statusCode = (HttpStatusCode)Convert.ToInt32(s[0]);
                if (String.IsNullOrEmpty(referrer) && s.Length > 1)
                {
                    referrer = s[1];
                }
            }

            Log.InfoFormat("url: {0}  referrer: {1}", this.Context.Url.AbsoluteUri, referrer);

            var viewModel = new NotFoundViewModel() { ReferralUri = referrer, StatusCode = statusCode };
            var template = FileService.LoadTemplate("notfound.mustache");

            this.Context.SetStatusCode(statusCode);
            template.Render(viewModel, this.Context.GetOutput(), null);
        }
    }
}
