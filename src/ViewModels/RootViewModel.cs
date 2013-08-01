namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;

    public class RootViewModel : ViewModelBase
    {
        public List<IssueViewModel> Issues { get; set; }

        public PaginationViewModel Page { get; set; }
    }
}
