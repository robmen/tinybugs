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

        public static IDbConnection Connect(bool readOnly = false)
        {
            return readOnly ? ConnectionString.OpenReadOnlyDbConnection() : ConnectionString.OpenDbConnection();
        }

        public static void Initialize(bool overwrite = false)
        {
            using (IDbConnection db = Connect())
            {
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

                Config config = new Config()
                {
                    Areas = new[] { "candle.exe", "light.exe" },
                    Releases = new[] { "v3.7", "v3.8", "v3.x", "v4.x" },
                };
                db.Insert(config);

                var fooUser = UserService.Create("foo@example.com", "bar.");
                fooUser.FullName = "Foo User";
                db.Save(fooUser);

                var barUser = UserService.Create("bar@example.com", "foo");
                barUser.UserName = "bar";
                barUser.FullName = "Bar User";
                db.Save(barUser);

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
                db.Save(issue);

                var issueSearch = new FullTextSearchIssue()
                {
                    DocId = db.GetLastInsertId(),
                    Text = issue.Text,
                    Title = issue.Title,
                };
                //db.SqlScalar<int>("INSERT INTO issue_text(docid, title, text) VALUES(@i, @t, @c)",
                //              new { i = db.GetLastInsertId(), t = issue.Title, c = issue.Text });
                db.Save(issueSearch);

                var issueOld = new Issue()
                {
                    AssignedToUserId = fooUser.Id,
                    CreatedByUserId = barUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Title = "This is the title of old feature.",
                    Type = IssueType.Feature,
                    Release = "v3.7",
                    Text = "This is the text of the feature. It is a little longer than the title and it's for older stuff.",
                    Votes = 1,
                };
                db.Save(issueOld);

                var issueOldComment = new IssueComment()
                {
                    IssueId = db.GetLastInsertId(),
                    CommentByUserId = barUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Text = "This is the text of the comment. It is a little longer to provide some detail about the feature request.",
                };
                db.Save(issueOldComment);

                var issueOldSearch = new FullTextSearchIssue()
                {
                    DocId = issueOldComment.IssueId,
                    Text = issueOld.Text,
                    Title = issueOld.Title,
                    Comments = issueOldComment.Text,
                };
                //db.SqlScalar<int>("INSERT INTO issue_text(docid, title, text) VALUES(@i, @t, @c)",
                //              new { i = db.GetLastInsertId(), t = issue.Title, c = issue.Text });
                db.Save(issueOldSearch);
            }
        }
    }
}
