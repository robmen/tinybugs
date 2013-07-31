namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;

    public class IssuesApiViewModel : ViewModelBase
    {
        public IEnumerable<IssueViewModel> Issues { get; set; }

        public PaginationViewModel Page { get; set; }
    }
}
