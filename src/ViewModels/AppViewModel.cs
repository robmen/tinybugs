namespace RobMensching.TinyBugs.ViewModels
{
    using RobMensching.TinyBugs.Services;

    public class AppViewModel
    {
        public string Path { get { return ConfigService.AppPath; } }

        public string Name { get { return ConfigService.AppName; } }
    }
}
