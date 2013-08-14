namespace RobMensching.TinyBugs.Models
{
    using System;

    public class QueryFilterColumn
    {
        public QueryFilterColumn(string filter)
        {
            string[] split = filter.Split(new char[] { ':', '=' }, 2);
            this.Column = split[0];
            this.Value = (split.Length > 1) ? split[1] : null;
        }

        public string Column { get; set; }

        public string Value { get; set; }
    }
}
