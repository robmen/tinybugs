namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class IssuesPaged
    {
        public List<CompleteIssue> Issues { get; set; }

        public int Page { get; set; }

        public int Pages { get; set; }

        public int PreviousPage { get; set; }

        public int NextPage { get; set; }

        public long Total { get; set; }
    }
}
