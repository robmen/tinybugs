namespace RobMensching.TinyBugs
{
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration;
    using RobMensching.TinyBugs.Services;

    public class Application : HttpApplication
    {
        protected void Application_OnStart()
        {
            ConnectionStringSettings conn = WebConfigurationManager.ConnectionStrings["db"] ?? new ConnectionStringSettings("db", "~/App_Data/bugs.sqlite", "SQLite");
            DataService.ConnectionString = Server.MapPath(conn.ConnectionString);

            DataService.Initialize();
            FileService.Initialize(Server.MapPath("~/"));
        }
    }
}
