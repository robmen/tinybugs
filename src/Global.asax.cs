namespace RobMensching.TinyBugs
{
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
    }
}
