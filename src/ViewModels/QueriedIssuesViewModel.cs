namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;

    public class QueriedIssuesViewModel
    {
        public List<IssueViewModel> Issues { get; set; }

        public int Total { get; set; }
    }
}
