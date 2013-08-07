namespace RobMensching.TinyBugs.Models
{
    public class Breadcrumb
    {
        public Breadcrumb()
        {
        }

        public Breadcrumb(string title, string url)
        {
            this.Title = title;
            this.Url = url;
        }

        public string Title { get; set; }

        public string Url { get; set; }
    }
}
