namespace RobMensching.TinyBugs.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using RobMensching.TinyBugs.Services;

    public class AppViewModel
    {
        public string Path { get { return ConfigService.AppPath; } }

        public string Name { get { return ConfigService.AppName; } }

        public IEnumerable<ReleaseViewModel> Releases { get { return ConfigService.Releases.Select(r => new ReleaseViewModel() { Name = r }); } }
    }
}
