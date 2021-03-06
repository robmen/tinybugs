﻿namespace tinyBugs.test
{
    using System;
    using System.Data;
    using System.Linq;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.OrmLite;
    using Xunit;

    public class DataFixture
    {
        [Fact]
        public void Create()
        {
            DataService.ConnectionString = "test.sqlite";

            var userGuid = Guid.NewGuid();
            using (IDbConnection db = DataService.Connect())
            {
                long issueId = -1;
                using (IDbTransaction tx = db.OpenTransaction())
                {
                    db.CreateTables(true, typeof(User), typeof(Issue), typeof(IssueComment));
                    db.Insert(new User() { Guid = userGuid, Email = "tinybugs@robmensching.com", FullName = "tinyBugs at RobMensching.com" });
                    long userId = db.GetLastInsertId();

                    db.Insert(new Issue() { AssignedToUserId = userId, CreatedByUserId = userId, CreatedAt = DateTime.UtcNow, Title = "Test bug", Type = IssueType.Bug, Status = IssueStatus.Open, Release = "v3.x", Text = "This is a test bug. It will have Markdown content in it." });
                    issueId = (int)db.GetLastInsertId();

                    tx.Commit();
                }

                Assert.Single(db.Select<Issue>().Select(i => i.Id), issueId);

                var user = db.FirstOrDefault<User>(u => u.Guid == userGuid);
                string us = db.GetLastSql();
                var issue = db.GetByIdParam<Issue>(issueId);
                string uss = db.GetLastSql();
                Assert.Equal(user.Id, issue.CreatedByUserId);
            }
        }
    }
}
