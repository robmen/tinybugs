namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;

    public class IssuesApiViewModel : ViewModelBase
    {
        public IEnumerable<CompleteIssue> Issues { get; set; }

        public Pagination Page { get; set; }
    }
}
