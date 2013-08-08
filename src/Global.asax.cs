namespace RobMensching.TinyBugs
{
    using System;
    using System.Web;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;
    using ServiceStack.Logging;
    using ServiceStack.Logging.NLogger;
    using ServiceStack.Text;

    public class Application : HttpApplication
    {
        private static bool FirstRequestInitialization = true;
        private static object FirstRequestInitializationLock = new object();

        protected void Application_OnStart()
        {
            LogManager.LogFactory = new NLogFactory();

            JsConfig.EmitCamelCaseNames = true;
            JsConfig<IssueType>.SerializeFn = (t => t.ToString().ToCamelCase());
            JsConfig<IssueStatus>.SerializeFn = (t => t.ToString().ToCamelCase());

            ConfigService.ApplicationInitialization(this);

            Routing.RegisterRoutes();
        }

        protected void Application_BeginRequest()
        {
            if (FirstRequestInitialization)
            {
                lock (FirstRequestInitializationLock)
                {
                    if (FirstRequestInitialization)
                    {
                        ConfigService.FirstRequestInitialiation(this);
                        FileService.PregenerateApp();
                        FirstRequestInitialization = false;
                    }
                }
            }
        }

        protected void Application_OnError(object sender, EventArgs e)
        {
            Application app = (Application)sender;
            Exception exception = Server.GetLastError().GetBaseException();

            LogManager.GetLogger("error").Error(String.Format("  page: {0}\r\n  referrer: {1}", app.Request.Url, app.Request.UrlReferrer), exception);
        }
    }
}
