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
        private static Breadcrumb[] breadcrumbs;

        static ConfigService()
        {
            areas = new string[0];
            releases = new string[0];
            breadcrumbs = new Breadcrumb[0];

            Mail = new MailConfig()
            {
                From = WebConfigurationManager.AppSettings["mail.from"],
                Server = WebConfigurationManager.AppSettings["mail.server"],
                Port = WebConfigurationManager.AppSettings["mail.port"] != null ? Int32.Parse(WebConfigurationManager.AppSettings["mail.port"]) : 0,
                Username = WebConfigurationManager.AppSettings["mail.username"],
                Password = WebConfigurationManager.AppSettings["mail.password"],
                RequireSsl = WebConfigurationManager.AppSettings["mail.ssl"] != null ? Boolean.Parse(WebConfigurationManager.AppSettings["mail.ssl"]) : false,
            };
        }

        /// <summary>
        /// Gets the connection string to use for tinyBugs.
        /// </summary>
        /// <remarks>This is the only property that can be accessed without calling the initialization routines.</remarks>
        public static ConnectionStringSettings ConnectionStringSettings { get { return WebConfigurationManager.ConnectionStrings["db"] ?? new ConnectionStringSettings("db", "~/App_Data/bugs.sqlite", "SQLite"); } }

        public static string AppName { get; private set; }

        public static string AppSubName { get; private set; }

        /// <summary>
        /// Gets the application url for tinyBugs. For example, `http://wixtoolset.org/bugs/` at http://wixtoolset.org/bugs/.
        /// </summary>
        /// <remarks>This property is invalid until the first request to the application is made.</remarks>
        public static string AppFullUrl { get; set; }

        /// <summary>
        /// Gets the application path for tinyBugs. For example, `/bugs/` at http://wixtoolset.org/bugs/.
        /// </summary>
        /// <remarks>This property is invalid until the first request to the application is made.</remarks>
        public static string AppPath { get; private set; }

        public static IEnumerable<string> Areas { get { return areas; } }

        public static IEnumerable<string> Releases { get { return releases; } }

        public static IEnumerable<Breadcrumb> ExternalBreadcrumbs { get { return breadcrumbs; } }

        public static MailConfig Mail { get; private set; }

        /// <summary>
        /// Gets the physical root path for tinyBugs.
        /// </summary>
        public static string RootPath { get; private set; }

        public static bool NoConfig { get; private set; }

        public static void ApplicationInitialization(Application application)
        {
            DataService.ConnectionString = application.Server.MapPath(ConfigService.ConnectionStringSettings.ConnectionString);
            DataService.Initialize();

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
                breadcrumbs = config.ExternalBreadcrumbs;
                Mail = new MailConfig()
                {
                    From = config.MailFrom ?? config.MailUsername,
                    Server = config.MailServer,
                    Port = config.MailPort,
                    RequireSsl = config.MailSsl,
                    Username = config.MailUsername,
                    Password = config.MailPassword,
                };
            }

            if (String.IsNullOrEmpty(AppName))
            {
                AppName = WebConfigurationManager.AppSettings["app.name"] ?? "tinyBugs";
                AppSubName = WebConfigurationManager.AppSettings["app.subname"] ?? "no issue is too small";
            }
        }
    }
}
