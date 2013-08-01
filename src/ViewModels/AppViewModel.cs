namespace RobMensching.TinyBugs.ViewModels
{
    using System.Web.Configuration;

    public class AppViewModel
    {
        public AppViewModel(string path = null)
        {
            this.Name = WebConfigurationManager.AppSettings["app.name"] ?? "tinyBugs";
            this.Path = path;
        }

        public string Path { get; set; }

        public string Name { get; set; }
    }
}
