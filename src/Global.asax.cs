namespace RobMensching.TinyBugs
{
    using System.Configuration;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.Text;
    using ServiceStack.Logging;
    using ServiceStack.Logging.NLogger;

    public class Application : HttpApplication
    {
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
    }
}
