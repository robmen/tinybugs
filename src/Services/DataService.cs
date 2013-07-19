namespace RobMensching.TinyBugs.Services
{
    using System.Data;
    using RobMensching.TinyBugs.Models;
    using ServiceStack.OrmLite;

    public static class DataService
    {
        static DataService()
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
            ConnectionString = ":memory:";
        }

        public static string ConnectionString { get; set; }

        public static IDbConnection Connect(bool readOnly = false)
        {
            return readOnly ? ConnectionString.OpenReadOnlyDbConnection() : ConnectionString.OpenDbConnection();
        }

        public static void Initialize(bool overwrite = false)
        {
            using (IDbConnection db = Connect())
            {
                db.CreateTables(overwrite, typeof(User), typeof(Issue), typeof(IssueUpdate));
            }
        }
    }
}
