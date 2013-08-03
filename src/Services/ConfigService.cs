namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web.Configuration;
    using RobMensching.TinyBugs.Models;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public static class ConfigService
    {
        private static string[] areas;
        private static string[] releases;

        /// <summary>
        /// Gets the connection string to use for tinyBugs.
        /// </summary>
        /// <remarks>This is the only property that can be accessed without calling the initialization routines.</remarks>
        public static ConnectionStringSettings ConnectionStringSettings { get { return WebConfigurationManager.ConnectionStrings["db"] ?? new ConnectionStringSettings("db", "~/App_Data/bugs.sqlite", "SQLite"); } }

        public static string FromEmail { get { return WebConfigurationManager.AppSettings["mail.from"] ?? "noreply@tinybugs.com"; } }

        public static string AppName { get; private set; }

        public static string AppSubName { get; private set; }

        /// <summary>
        /// Gets the application url for tinyBugs. For example, `http://wixtoolset/bugs/` at http://wixtoolset/bugs/.
        /// </summary>
        /// <remarks>This property is invalid until the first request to the application is made.</remarks>
        public static string AppFullUrl { get; set; }

        /// <summary>
        /// Gets the application path for tinyBugs. For example, `/bugs/` at http://wixtoolset/bugs/.
        /// </summary>
        /// <remarks>This property is invalid until the first request to the application is made.</remarks>
        public static string AppPath { get; private set; }

        public static IEnumerable<string> Areas { get { return areas; } }

        public static IEnumerable<string> Releases { get { return releases; } }

        /// <summary>
        /// Gets the physical root path for tinyBugs.
        /// </summary>
        public static string RootPath { get; private set; }

        public static bool NoConfig { get; private set; }

        public static void ApplicationInitialization(Application application)
        {
            DataService.ConnectionString = application.Server.MapPath(ConfigService.ConnectionStringSettings.ConnectionString);
            DataService.Initialize(true);

            using (var db = DataService.Connect(true))
            {
                var sql = DataService.DialectProvider.ExpressionVisitor<Config>();
                var config = db.Select<Config>(sql.OrderByDescending(c => c.UpdatedAt).Limit(1)).FirstOrDefault();
                InitializeWithConfig(config);
            }

            RootPath = application.Server.MapPath("~/");
        }

        public static void FirstRequestInitialiation(Application application)
        {
            AppFullUrl = application.Context.Request.Url.GetLeftPart(UriPartial.Authority) + application.Context.Request.ApplicationPath.WithTrailingSlash();

            AppPath = application.Context.Request.ApplicationPath.WithTrailingSlash();
        }

        public static void InitializeWithConfig(Config config)
        {
            if (config == null)
            {
                NoConfig = true;
            }
            else
            {
                AppName = config.ApplicationName;
                AppSubName = config.ApplicationSubName;
                areas = config.Areas;
                releases = config.Releases;
            }

            if (String.IsNullOrEmpty(AppName))
            {
                AppName = WebConfigurationManager.AppSettings["app.name"] ?? "tinyBugs";
                AppSubName = WebConfigurationManager.AppSettings["app.subname"] ?? "no issue is too small";
            }
        }
    }
}
