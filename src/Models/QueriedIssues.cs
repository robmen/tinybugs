namespace RobMensching.TinyBugs.Models
{
    using System.Collections.Generic;

    public class QueriedIssues
    {
        public List<CompleteIssue> Issues { get; set; }

        public int Total { get; set; }
    }
}
