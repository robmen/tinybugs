namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public static class QueryService
    {
        private const string IssueSqlQueryTemplate = @"
SELECT *, u1.Email AS AssignedToUserEmail, u1.UserName AS AssignedToUserName,
          u2.Email AS CreatedByUserEmail, u2.UserName AS CreatedByUserName
FROM Issue
LEFT JOIN User AS u1 ON Issue.AssignedToUserId=u1.Id
LEFT JOIN User AS u2 ON Issue.CreatedByUserId=u2.Id
/**join**/ /**leftjoin**/ /**where**/ /**orderby**/";

        private const string IssueCountSqlQueryTemplate = @"
SELECT COUNT(Issue.Id)
FROM Issue
LEFT JOIN User AS u1 ON Issue.AssignedToUserId=u1.Id
LEFT JOIN User AS u2 ON Issue.CreatedByUserId=u2.Id
/**join**/ /**leftjoin**/ /**where**/ /**orderby**/";

        private const string CommentSqlQueryTemplate = @"
SELECT *, User.Email AS CommentByUserEmail, User.UserName AS CommentByUserName
FROM IssueComment
LEFT JOIN User ON IssueComment.CommentByUserId=User.Id
/**join**/ /**leftjoin**/ /**where**/ /**orderby**/";


        private static Dictionary<string, string> AllowedColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "id", "Issue.Id" },
            { "assigned", "u1.UserName" },
            { "assignedto", "u1.UserName" },
            { "assignedtoemail", "u1.Email" },
            { "assignedtouser", "u1.UserName" },
            { "assignedtoname", "u1.UserName" },
            { "assignedtousername", "u1.UserName" },
            { "createdby", "u2.UserName" },
            { "createdbyemail", "u2.Email" },
            { "createdbyuser", "u2.UserName" },
            { "createdbyname", "u2.UserName" },
            { "createdbyusername", "u2.UserName" },
            { "createdat", "CreatedAt" },
            { "created", "CreatedAt" },
            { "updatedat", "UpdatedAt" },
            { "updated", "UpdatedAt" },
            { "status", "Status" },
            { "resolution", "Resolution" },
            { "area", "Area" },
            { "milestone", "Release" },
            { "release", "Release" },
            { "tag", "Tags" },
            { "tags", "Tags" },
            { "content", "Text" },
            { "text", "Text" },
            { "title", "Title" },
            { "type", "Type" },
            { "private", "Private" },
            { "vote", "Votes" },
            { "votes", "Votes" },
        };

        public static Query ParseQuery(string query)
        {
            NameValueCollection coll = HttpUtility.ParseQueryString(query);
            return ParseQuery(coll);
        }

        public static Query ParseQuery(NameValueCollection query)
        {
            string[] templates = query.GetValues("template");
            string[] filters = query.GetValues("filter");
            string[] sorts = query.GetValues("sort");
            string[] searches = query.GetValues("search");
            string[] pages = query.GetValues("page");
            string[] counts = query.GetValues("count");

            return new Query()
            {
                Filters = (filters != null) ? filters.Select(f => new QueryFilterColumn(f)).ToArray() : null,
                Sorts = (sorts != null) ? sorts.Select(s => new QuerySortColumn(s)).ToArray() : null,
                Searches = (searches != null) ? searches.ToArray() : null,
                Page = (pages != null) ? Convert.ToInt32(pages[pages.Length - 1]) : 1,
                Count = (counts != null) ? Math.Min(Convert.ToInt32(counts[counts.Length - 1]), 500) : 250,
                Template = (templates != null) ? templates[templates.Length - 1] : null,
            };
        }

        public static string RecreateQueryString(Query query)
        {
            StringBuilder sb = new StringBuilder("?");
            if (query.Filters != null)
            {
                string[] filters = query.Filters.Select(f => String.Concat("filter=", f.Column.UrlEncode(), ":", f.Value.UrlEncode())).ToArray();
                sb.Append(String.Join("&", filters));
                sb.Append("&");
            }

            if (query.Sorts != null)
            {
                string[] sorts = query.Sorts.Select(s => String.Concat("sort=", s.Column.UrlEncode(), s.Direction == QuerySortDirection.Descending ? ":desc" : null)).ToArray();
                sb.Append(String.Join("&", sorts));
                sb.Append("&");
            }

            if (query.Searches != null)
            {
                string[] searches = query.Searches.Select(s => String.Concat("search=", s.UrlEncode())).ToArray();
                sb.Append(String.Join("&", searches));
                sb.Append("&");
            }

            sb.Append("count=");
            sb.Append(query.Count);

            return sb.ToString();
        }

        public static QueriedIssuesViewModel QueryIssues(Query query)
        {
            SqlBuilder sql = new SqlBuilder();
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (query.Filters != null)
            {
                foreach (var filter in query.Filters)
                {
                    ProcessFilter(filter, sql, parameters);
                }
            }

            if (query.Sorts != null)
            {
                foreach (var sort in query.Sorts)
                {
                    ProcessSort(sort, sql, parameters);
                }
            }
            else
            {
                ProcessSort(new QuerySortColumn("id"), sql, parameters);
            }

            if (query.Searches != null)
            {
                ProcessSearch(String.Join(" ", query.Searches), sql, parameters);
            }

            // Prepare the SQL statements.
            var countTemplate = sql.AddTemplate(IssueCountSqlQueryTemplate);
            var issuesTemplate = sql.AddTemplate(IssueSqlQueryTemplate);
            string rawSql = issuesTemplate.RawSql + " LIMIT " + query.Count;
            if (query.Page > 1)
            {
                rawSql += " OFFSET " + (query.Page - 1) * query.Count;
            }

            using (var db = DataService.Connect())
            {
                int total = db.SqlScalar<int>(countTemplate.RawSql, parameters);
                return new QueriedIssuesViewModel()
                {
                    Issues = db.Query<IssueViewModel>(rawSql, parameters),
                    Total = total,
                };
            }
        }

        public static List<IssueCommentViewModel> CommentsForIssue(int issueId)
        {
            using (var db = DataService.Connect(true))
            {
                return CommentsForIssueUsingDb(issueId, db);
            }
        }

        public static List<IssueCommentViewModel> CommentsForIssueUsingDb(int issueId, IDbConnection db)
        {
            SqlBuilder sql = new SqlBuilder();
            sql.Where("IssueId=@issueId", new { issueId = issueId });
            var commentTemplate = sql.AddTemplate(CommentSqlQueryTemplate);

            return db.Query<IssueCommentViewModel>(commentTemplate.RawSql, commentTemplate.Parameters);
        }

        public static IssueViewModel AssignAreasReleasesAndTypesToIssue(IssueViewModel issue)
        {
            issue.Areas = ConfigService.Areas
                            .Select(a => new OptionViewModel() { Selected = a.Equals(issue.Area, StringComparison.OrdinalIgnoreCase), Text = a, Value = a })
                            .ToList();

            issue.Releases = ConfigService.Releases
                               .Select(r => new OptionViewModel() { Selected = r.Equals(issue.Release, StringComparison.OrdinalIgnoreCase), Text = r, Value = r })
                               .ToList();

            issue.Types = ConfigService.Types
                               .Select(t => new OptionViewModel() { Selected = (t == issue.Type), Text = t.ToString(), Value = t.ToString() })
                               .ToList();
            return issue;
        }

        public static bool TryGetUser(string nameOrEmail, out User user)
        {
            return TryGetUser(Guid.Empty, nameOrEmail, out user);
        }

        public static bool TryGetUser(Guid currentUserGuid, string nameOrEmail, out User user)
        {
            nameOrEmail = String.IsNullOrEmpty(nameOrEmail) ? String.Empty : nameOrEmail.ToLowerInvariant();

            using (var db = DataService.Connect(true))
            {
                user = (currentUserGuid != Guid.Empty && "[me]".Equals(nameOrEmail, StringComparison.OrdinalIgnoreCase)) ?
                        db.FirstOrDefault<User>(u => u.Guid == currentUserGuid) :
                        db.FirstOrDefault<User>(u => u.UserName == nameOrEmail || u.Email == nameOrEmail);
            }

            return user != null;
        }

        public static bool TryGetUserByName(Guid currentUserGuid, string name, out User user)
        {
            using (var db = DataService.Connect(true))
            {
                user = (currentUserGuid != Guid.Empty && "[me]".Equals(name, StringComparison.OrdinalIgnoreCase)) ?
                        db.FirstOrDefault<User>(u => u.Guid == currentUserGuid) :
                        db.FirstOrDefault<User>(u => u.UserName == name);
            }

            return user != null;
        }

        public static bool TryGetIssueWithComments(long issueId, out IssueViewModel issue)
        {
            using (var db = DataService.Connect(true))
            {
                return TryGetIssueWithCommentsUsingDb(issueId, db, out issue);
            }
        }

        public static bool TryGetIssueWithCommentsUsingDb(long issueId, IDbConnection db, out IssueViewModel issue)
        {
            SqlBuilder sql = new SqlBuilder();
            sql.Where("Issue.Id=@id", new { id = issueId });
            var issueTemplate = sql.AddTemplate(IssueSqlQueryTemplate);

            issue = db.Query<IssueViewModel>(issueTemplate.RawSql, issueTemplate.Parameters).SingleOrDefault();
            if (issue != null)
            {
                issue.Comments = CommentsForIssueUsingDb(issue.Id, db);

                QueryService.AssignAreasReleasesAndTypesToIssue(issue);
            }

            return issue != null;
        }

        private static void ProcessFilter(QueryFilterColumn filter, SqlBuilder sql, Dictionary<string, object> parameters)
        {
            string column;
            if (AllowedColumns.TryGetValue(filter.Column, out column))
            {
                string parameterName = "p" + parameters.Keys.Count;

                sql.Where(column + "=@" + parameterName);
                parameters.Add(parameterName, filter.Value);
            }
        }

        private static void ProcessSort(QuerySortColumn sort, SqlBuilder sql, Dictionary<string, object> parameters)
        {
            string column;
            if (AllowedColumns.TryGetValue(sort.Column, out column))
            {
                sql.OrderBy(column + (sort.Direction == QuerySortDirection.Descending ? " DESC" : String.Empty));
            }
        }

        private static void ProcessSearch(string search, SqlBuilder sql, Dictionary<string, object> parameters)
        {
            sql.Join(" FullTextSearchIssue ON Issue.Id=FullTextSearchIssue.docid");
            sql.Where("FullTextSearchIssue MATCH @search");
            parameters.Add("search", search);
        }
    }
}
