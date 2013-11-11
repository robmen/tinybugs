namespace RobMensching.TinyBugs.Services
{
    using System;
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

        public static IOrmLiteDialectProvider DialectProvider { get { return OrmLiteConfig.DialectProvider; } }

        public static IDbConnection Connect(bool readOnly = false)
        {
            return readOnly ? ConnectionString.OpenReadOnlyDbConnection() : ConnectionString.OpenDbConnection();
        }

        public static void Initialize(bool overwrite = false)
        {
            using (IDbConnection db = Connect())
            {
                //bool userTableExists = db.TableExists("User");
                bool fullTextSearchExists = db.TableExists("FullTextSearchIssue");
                if (overwrite && fullTextSearchExists)
                {
                    db.ExecuteSql("DROP TABLE FullTextSearchIssue");
                    fullTextSearchExists = false;
                }

                db.CreateTables(overwrite, typeof(User), typeof(Config), typeof(Issue), typeof(IssueComment));
                if (!fullTextSearchExists)
                {
                    db.ExecuteSql("CREATE VIRTUAL TABLE FullTextSearchIssue USING fts4(Title, Text, Comments)");
                }
            }
        }
    }
}
