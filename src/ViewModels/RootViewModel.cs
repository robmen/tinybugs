namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;

    public class RootViewModel : ViewModelBase
    {
        public List<CompleteIssue> Issues { get; set; }

        public Pagination Page { get; set; }
    }
}
