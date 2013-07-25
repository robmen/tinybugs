namespace tinyBugs.test
{
    using System;
    using System.Data;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.OrmLite;
    using Xunit;

    public class QueryFixture
    {
        [Fact]
        public void TestSqlBuilder()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            using (var db = DataService.Connect())
            {
                string releaseValue = "v3.8";
                SqlBuilder sb = new SqlBuilder();
                sb.Select("*");
                sb.Select("u1.Email AS AssignedToUserEmail, u1.Name AS AssignedToUserName");
                sb.Select("u2.Email AS CreatedByUserEmail, u2.Name AS CreatedByUserName");
                sb.Join(" User AS u1 ON Issue.AssignedToUserId=u1.Id");
                sb.Join(" User AS u2 ON Issue.CreatedByUserId=u2.Id");
                sb.Where("Release=@a", new { a = releaseValue });
                sb.Where("Votes>@b", new { b = 3});
                sb.OrderBy("Issue.Id");

                var t = new SqlBuilder.Template(sb, @"SELECT /**select**/ FROM Issue /**join**/ /**leftjoin**/ /**where**/ /**orderby**/", null);
                var iss = db.Query<CompleteIssue>(t.RawSql, t.Parameters);
                string sql = db.GetLastSql();
                Assert.NotEmpty(iss);
                Assert.Equal("foo@example.com", iss[0].AssignedToUserEmail);
                Assert.Equal("bar@example.com", iss[0].CreatedByUserEmail);
                Assert.Equal("This is the title.", iss[0].Title);

                //var jsb = new JoinSqlBuilder<CompleteIssue, Issue>();
                //jsb = jsb.Join<Issue, User>(i => i.AssignedToUserId, u => u.Id)
                //         .Join<Issue, User>(i => i.CreatedByUserId, x => x.Id)
                //         .Where<Issue>(i => i.Release == releaseValue);
                //string foo = jsb.ToSql();
                //var issues = db.Query<CompleteIssue>(foo);
                //string sql2 = db.GetLastSql();
                //Assert.NotEmpty(issues);
            }
        }

        [Fact]
        public void FilterQuery()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?filter=release:v3.8");
            var issues = QueryService.QueryIssues(q);
            Assert.Equal("foo@example.com", issues.Issues[0].AssignedToUserEmail);
            Assert.Equal("bar@example.com", issues.Issues[0].CreatedByUserEmail);
            Assert.Equal("This is the title.", issues.Issues[0].Title);
            issues.Issues.ForEach(i => Assert.Equal("v3.8", i.Release));
        }

        [Fact]
        public void FilterQueryUsingAlias()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?filter=milestone:v3.8");
            var issues = QueryService.QueryIssues(q);

            Assert.NotEmpty(issues.Issues);
            issues.Issues.ForEach(i => Assert.Equal("v3.8", i.Release));
        }

        [Fact]
        public void SortQuery()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?sort=release");
            var issues = QueryService.QueryIssues(q);

            Assert.Equal("v3.7", issues.Issues[0].Release);
            Assert.Equal("v3.8", issues.Issues[1].Release);
        }

        [Fact]
        public void SortQueryDescending()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?sort=release:desc");
            var issues = QueryService.QueryIssues(q).Issues;

            Assert.Equal("v3.8", issues[0].Release);
            Assert.Equal("v3.7", issues[1].Release);
        }

        [Fact]
        public void SearchQuery()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?search=old");
            var issues = QueryService.QueryIssues(q);

            Assert.NotEmpty(issues.Issues);
            issues.Issues.ForEach(i => Assert.Equal("v3.7", i.Release));
        }

        [Fact]
        public void SearchQuery2()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?search=\"little%20longer\"");
            var issues = QueryService.QueryIssues(q);

            Assert.Equal(2, issues.Issues.Count);
        }

        [Fact]
        public void SearchQueryEmail()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q = QueryService.ParseQuery("?filter=assignedto=foo@example.com");
            var issues = QueryService.QueryIssues(q).Issues;

            issues.ForEach(i => Assert.Equal("foo@example.com", i.AssignedToUserEmail));

            var qc = QueryService.ParseQuery("?filter=createdby=bar@example.com");
            var ic = QueryService.QueryIssues(qc).Issues;

            ic.ForEach(i => Assert.Equal("bar@example.com", i.CreatedByUserEmail));
        }

        [Fact]
        public void PagedQuery()
        {
            DataService.ConnectionString = "test.sqlite";
            DataService.Initialize(true);

            var q1 = QueryService.ParseQuery("?page=1&count=1");
            var i1 = QueryService.QueryIssues(q1);
            Assert.Single(i1.Issues);
            Assert.Equal("v3.8", i1.Issues[0].Release);

            var q2 = QueryService.ParseQuery("?page=2&count=1");
            var i2 = QueryService.QueryIssues(q2).Issues;
            Assert.Single(i2);
            Assert.Equal("v3.7", i2[0].Release);

            var q3 = QueryService.ParseQuery("?page=3&count=1");
            var i3 = QueryService.QueryIssues(q3).Issues;
            Assert.Empty(i3);
        }

        [Fact]
        public void RecreateQuery()
        {
            var q = QueryService.ParseQuery("?filter=createdby=bar@example.com");
            var s = QueryService.RecreateQueryString(q);
            Assert.Equal("?filter=createdby:bar%40example.com&count=250", s);
        }

        private static IDbConnection OpenDatabaseWithTestData()
        {
            var db = ":memory:".OpenDbConnection();
            db.CreateTables(false, typeof(User), typeof(Issue), typeof(IssueUpdate));

            var fooUser = UserService.Create("foo@example.com", "bar.");
            fooUser.Name = "Foo User";
            db.Insert(fooUser);

            var barUser = UserService.Create("bar@example.com", "foo");
            barUser.Name = "Bar User";
            db.Insert(barUser);

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
            db.Insert(issueOld);

            return db;
        }
    }
}
