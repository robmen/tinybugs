namespace RobMensching.TinyBugs.ViewModels
{
    using System;
    using System.Linq;

    public class BreadcrumbsViewModel
    {
        public BreadcrumbsViewModel(params BreadcrumbViewModel[] crumbs)
        {
            this.Parents = crumbs.Take(crumbs.Length - 1).ToArray();
            this.Current = crumbs[crumbs.Length - 1];
        }

        public BreadcrumbViewModel[] Parents { get; set; }

        public BreadcrumbViewModel Current { get; set; }
    }
}
