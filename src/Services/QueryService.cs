namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Web;
    using RobMensching.TinyBugs.Models;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public static class QueryService
    {
        private const string IssueSqlQueryTemplate = @"
SELECT *, u1.Email AS AssignedToUserEmail, u1.Name AS AssignedToUserName,
          u2.Email AS CreatedByUserEmail, u2.Name AS CreatedByUserName
FROM Issue
INNER JOIN User AS u1 ON Issue.AssignedToUserId=u1.Id
INNER JOIN User AS u2 ON Issue.CreatedByUserId=u2.Id
/**join**/ /**leftjoin**/ /**where**/ /**orderby**/";

        private const string IssueCountSqlQueryTemplate = @"
SELECT COUNT(Issue.Id)
FROM Issue
INNER JOIN User AS u1 ON Issue.AssignedToUserId=u1.Id
INNER JOIN User AS u2 ON Issue.CreatedByUserId=u2.Id
/**join**/ /**leftjoin**/ /**where**/ /**orderby**/";

        private static Dictionary<string, string> AllowedColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "id", "Issue.Id" },
            { "assignedto", "u1.Email" /*"AssignedToUserEmail"*/ },
            { "assignedtoemail", "u1.Email" /*"AssignedToUserEmail"*/ },
            { "assignedtouser", "u1.Name" /*"AssignedToUserName"*/ },
            { "assignedtoname", "u1.Name" /*"AssignedToUserName"*/ },
            { "assignedtousername", "u1.Name" /*"AssignedToUserName"*/ },
            { "createdby", "u2.Email" /*"CreatedByUserEmail"*/ },
            { "createdbyemail", "u2.Email" /*"CreatedByUserEmail"*/ },
            { "createdbyuser", "u2.Name" /*"CreatedByUserName"*/ },
            { "createdbyname", "u2.Name" /*"CreatedByUserName"*/ },
            { "createdbyusername", "u2.Name" /*"CreatedByUserName"*/ },
            { "createdat", "CreatedAt" },
            { "created", "CreatedAt" },
            { "updatedat", "UpdatedAt" },
            { "updated", "UpdatedAt" },
            { "status", "Status" },
            { "resolution", "Resolution" },
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

        public static IssuesPaged QueryIssues(Query query)
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
                ProcessSort(new QuerySortColumn("created"), sql, parameters);
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
                long total = db.SqlScalar<long>(countTemplate.RawSql, parameters);
                int pages = (int)((total + query.Count - 1) / query.Count);
                return new IssuesPaged()
                {
                    Issues = db.Query<CompleteIssue>(rawSql, parameters),
                    Page = (query.Page == 0) ? 1 : query.Page,
                    Pages = pages,
                    PreviousPage = (query.Page > 1) ? Math.Min(query.Page - 1, pages) : 0,
                    NextPage = (query.Page < pages) ? query.Page + 1 : 0,
                    Total = total,
                };
            }
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
