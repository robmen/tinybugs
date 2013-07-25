namespace RobMensching.TinyBugs.Models
{
    using System;

    public enum QuerySortDirection
    {
        Ascending,
        Descending,
    }

    public class QuerySortColumn
    {
        public QuerySortColumn(string sort)
        {
            string[] split = sort.Split(new char[] { ':', '=' }, 2);
            this.Column = split[0];
            this.Direction = (split.Length > 1 && split[1].Equals("desc", StringComparison.OrdinalIgnoreCase)) ? QuerySortDirection.Descending : QuerySortDirection.Ascending;
        }
        public string Column { get; set; }

        public QuerySortDirection Direction { get; set; }
    }
}
