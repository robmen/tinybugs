namespace RobMensching.TinyBugs.Models
{
    // ?template=name&filter=column:value&sort=column:desc&search=query&page=#&count=#
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
