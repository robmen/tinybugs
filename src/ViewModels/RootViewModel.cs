namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;

    public class RootViewModel
    {
        public RootViewModel()
        {
        }

        public List<CompleteIssue> Issues { get; set; }

        public int Page { get; set; }

        public int Pages { get; set; }

        public long Total { get; set; }

        public string PageUriPrefix { get; set; }

        public string PreviousPageUri { get; set; }

        public string NextPageUri { get; set; }
    }
}
