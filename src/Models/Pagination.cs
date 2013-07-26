namespace RobMensching.TinyBugs.Models
{
    using System;

    public class Pagination
    {
        public Pagination(int page, int perPage, int totalItems, string pagedUrlPrefix = null)
        {
            this.Current = (page == 0) ? 1 : page;
            this.ItemsPerPage = perPage;
            this.ItemsTotal = totalItems;
            this.Total = (totalItems + perPage - 1) / perPage;
            this.Previous = (page > 1) ? Math.Min(page - 1, this.Total).ToString() : null;
            this.Next = (page < this.Total) ? (page + 1).ToString() : null;

            this.PagedUrlPrefix = pagedUrlPrefix;
            this.PreviousUrl = (!String.IsNullOrEmpty(this.Previous) && !String.IsNullOrEmpty(this.PagedUrlPrefix)) ? this.PagedUrlPrefix + this.Previous : null;
            this.NextUrl = (!String.IsNullOrEmpty(this.Next) && !String.IsNullOrEmpty(this.PagedUrlPrefix)) ? this.PagedUrlPrefix + this.Next : null;
        }

        public int Current { get; set; }

        public int Total { get; set; }

        public int ItemsPerPage { get; set; }

        public int ItemsTotal { get; set; }

        public string PagedUrlPrefix { get; set; }

        public string Previous { get; set; }

        public string PreviousUrl { get; set; }

        public string Next { get; set; }

        public string NextUrl { get; set; }
    }
}
