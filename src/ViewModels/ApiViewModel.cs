namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;

    public class ApiViewModel
    {
        public ApiViewModel()
        {
        }

        public IEnumerable<CompleteIssue> Issues { get; set; }

        public Pagination Page { get; set; }
    }
}
