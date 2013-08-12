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
#if DEBUG
            overwrite = true;
#endif

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

#if DEBUG
                Config config = new Config()
                {
                    Areas = new[] { "candle.exe", "light.exe" },
                    Releases = new[] { "v3.7", "v3.8", "v3.x", "v4.x" },
                };
                db.Insert(config);

                var fooUser = UserService.Create("foo@example.com", "bar.");
                fooUser.FullName = "Foo User";
                fooUser.Role = UserRole.Admin;
                db.Insert(fooUser);
                fooUser.Id = db.GetLastInsertId();

                var barUser = UserService.Create("bar@example.com", "foo");
                barUser.UserName = "bar";
                barUser.FullName = "Bar User";
                db.Insert(barUser);
                barUser.Id = db.GetLastInsertId();

                var bazUser = UserService.Create("baz@example.com", "foo");
                bazUser.UserName = "baz";
                bazUser.FullName = "Baz User";
                bazUser.Role = UserRole.User;
                db.Insert(bazUser);
                bazUser.Id = db.GetLastInsertId();

                var issue = new Issue()
                {
                    AssignedToUserId = fooUser.Id,
                    CreatedByUserId = barUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Title = "This is the title.",
                    Type = IssueType.Bug,
                    Release = "v3.8",
                    Text = "This is the text of the bug. It is a little longer than the title.",
                    Votes = 4,
                };
                db.Insert(issue);
                issue.Id = db.GetLastInsertId();

                var issueSearch = new FullTextSearchIssue()
                {
                    DocId = issue.Id,
                    Text = issue.Text,
                    Title = issue.Title,
                };
                //db.SqlScalar<int>("INSERT INTO issue_text(docid, title, text) VALUES(@i, @t, @c)",
                //              new { i = db.GetLastInsertId(), t = issue.Title, c = issue.Text });
                db.Insert(issueSearch);

                var issueOld = new Issue()
                {
                    CreatedByUserId = fooUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Title = "This is the title of old feature.",
                    Type = IssueType.Feature,
                    Release = "v3.7",
                    Text = "This is the text of the feature. It is a little longer than the title and it's for older stuff.",
                    Votes = 1,
                };
                db.Insert(issueOld);
                issueOld.Id = db.GetLastInsertId();

                var issueOldComment = new IssueComment()
                {
                    IssueId = issueOld.Id,
                    CommentByUserId = barUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Text = "This is the text of the comment. It is a little longer to provide some detail about the feature request.",
                };
                db.Insert(issueOldComment);
                issueOldComment.Id = db.GetLastInsertId();

                var issueOldSearch = new FullTextSearchIssue()
                {
                    DocId = issueOldComment.IssueId,
                    Text = issueOld.Text,
                    Title = issueOld.Title,
                    Comments = issueOldComment.Text,
                };
                //db.SqlScalar<int>("INSERT INTO issue_text(docid, title, text) VALUES(@i, @t, @c)",
                //              new { i = db.GetLastInsertId(), t = issue.Title, c = issue.Text });
                db.Insert(issueOldSearch);
#endif
            }
        }
    }
}
