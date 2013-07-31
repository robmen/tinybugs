namespace RobMensching.TinyBugs
{
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.Logging;
    using ServiceStack.Logging.NLogger;
    using ServiceStack.Text;

    public class Application : HttpApplication
    {
        private static bool FirstRequestInitialization = false;
        private static object FirstRequestInitializationLock = new object();

        protected void Application_OnStart()
        {
            LogManager.LogFactory = new NLogFactory();

            JsConfig.EmitCamelCaseNames = true;
            JsConfig<IssueType>.SerializeFn = (t => t.ToString().ToCamelCase());
            JsConfig<IssueStatus>.SerializeFn = (t => t.ToString().ToCamelCase());

            ConnectionStringSettings conn = WebConfigurationManager.ConnectionStrings["db"] ?? new ConnectionStringSettings("db", "~/App_Data/bugs.sqlite", "SQLite");
            DataService.ConnectionString = Server.MapPath(conn.ConnectionString);

            DataService.Initialize(true);
            FileService.Initialize(Server.MapPath("~/"));
        }

        protected void Application_BeginRequest()
        {
            if (!FirstRequestInitialization)
            {
                lock (this)
                {
                    if (!FirstRequestInitialization)
                    {
                        var app = new AppViewModel()
                        {
                            Name = WebConfigurationManager.AppSettings["app.name"] ?? "tinyBld",
                            Path = this.Context.Request.ApplicationPath.WithTrailingSlash(),
                        };

                        FileService.PregenerateApp(app);
                        FirstRequestInitialization = true;
                    }
                }
            }
        }
    }
}
