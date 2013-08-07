namespace RobMensching.TinyBugs.ViewModels
{
    using System;
    using System.Linq;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;

    public class BreadcrumbsViewModel
    {
        public BreadcrumbsViewModel(params Breadcrumb[] crumbs)
        {
            this.Parents = ConfigService.ExternalBreadcrumbs.Concat(crumbs.Take(crumbs.Length - 1)).ToArray();
            this.Current = crumbs[crumbs.Length - 1];
        }

        public Breadcrumb[] Parents { get; set; }

        public Breadcrumb Current { get; set; }
    }
}
