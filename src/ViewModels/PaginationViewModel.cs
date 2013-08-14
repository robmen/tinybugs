namespace RobMensching.TinyBugs.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class PaginationViewModel
    {
        public PaginationViewModel(int page, int perPage, int totalItems, string pagedUrlPrefix = null)
        {
            this.Current = (page == 0) ? 1 : page;
            this.ItemsPerPage = perPage;
            this.ItemsTotal = totalItems;
            this.Total = (totalItems + perPage - 1) / perPage;

            this.PagedUrlPrefix = pagedUrlPrefix;

            int width = 10;
            int start = Math.Max(1, this.Current - width / 2);
            int end = Math.Min(this.Total, start + width);

            this.First = new Page() { Number = 1, Active = false, Disabled = (page == 1), Url = !String.IsNullOrEmpty(this.PagedUrlPrefix) ? this.PagedUrlPrefix + "1" : null, };
            this.Previous = new Page() { Number = 1, Active = false, Disabled = (page == 1), Url = !String.IsNullOrEmpty(this.PagedUrlPrefix) ? this.PagedUrlPrefix + Math.Min(page - 1, this.Total) : null, };
            this.Next = new Page() { Number = Math.Min(page + 1, this.Total), Active = false, Disabled = (page == this.Total), Url = !String.IsNullOrEmpty(this.PagedUrlPrefix) ? this.PagedUrlPrefix + Math.Min(page + 1, this.Total) : null, };
            this.Last = new Page() { Number = this.Total, Active = false, Disabled = (page == this.Total), Url = !String.IsNullOrEmpty(this.PagedUrlPrefix) ? this.PagedUrlPrefix + this.Total : null, };

            if (end > start)
            {
                this.Pages = new List<Page>(end - start);
                for (int i = start; i <= end; ++i)
                {
                    this.Pages.Add(new Page() { Number = i, Active = (this.Current == i), Url = !String.IsNullOrEmpty(this.PagedUrlPrefix) ? this.PagedUrlPrefix + i : null, });
                }
            }
        }

        public int Current { get; set; }

        public int Total { get; set; }

        public int ItemsPerPage { get; set; }

        public int ItemsTotal { get; set; }

        public List<Page> Pages { get; private set; }

        public string PagedUrlPrefix { get; set; }

        public Page First { get; set; }

        public Page Previous { get; set; }

        public Page Next { get; set; }

        public Page Last { get; set; }

        public class Page
        {
            public int Number { get; set; }

            public bool Active { get; set; }

            public bool Disabled { get; set; }

            public string Url { get; set; }
        }
    }
}
