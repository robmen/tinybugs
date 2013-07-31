namespace RobMensching.TinyBugs.Models
{
    public class Query
    {
        public string Template { get; set; }

        public QueryFilterColumn[] Filters { get; set; }

        public QuerySortColumn[] Sorts { get; set; }

        public string[] Searches { get; set; }

        public int Page { get; set; }

        public int Count { get; set; }
    }
}
