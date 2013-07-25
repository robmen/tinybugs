namespace RobMensching.TinyBugs.Models
{
    public class FullTextSearchIssue
    {
        public long DocId { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string Comments { get; set; }
    }
}
