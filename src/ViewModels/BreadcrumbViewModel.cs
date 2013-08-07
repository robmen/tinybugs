namespace RobMensching.TinyBugs.ViewModels
{
    public class BreadcrumbViewModel
    {
        public BreadcrumbViewModel()
        {
        }

        public BreadcrumbViewModel(string title, string url)
        {
            this.Title = title;
            this.Url = url;
        }

        public string Title { get; set; }

        public string Url { get; set; }
    }
}
