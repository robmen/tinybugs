namespace RobMensching.TinyBugs.Services
{
    using System.Collections.Concurrent;
    using System.IO;
    using Nustache.Core;
    using RobMensching.TinyBugs.ViewModels;

    public static class FileService
    {
        private static ConcurrentDictionary<string, Template> CompiledTemplates = new ConcurrentDictionary<string, Template>();

        public static Template LoadTemplate(string path)
        {
            Template template;
            if (!CompiledTemplates.TryGetValue(path, out template))
            {
                template = new Template();
                using (var reader = FileService.ReadFile(path))
                {
                    template.Load(reader);
                }

                //if (!CompiledTemplates.TryAdd(path, template))
                //{
                //    template = CompiledTemplates[path];
                //}
            }

            return template;
        }

        public static TextReader ReadFile(string path, bool cache = false)
        {
            string fullPath = Path.Combine(ConfigService.RootPath, path);
            return File.OpenText(fullPath);
        }

        public static void WriteIssue(IssueViewModel issue, AppViewModel app = null)
        {
            string file = Path.Combine(ConfigService.RootPath, issue.Id.ToString() + "\\index.html");

            Template template = LoadTemplate("bug.mustache");
            RenderTemplateToFile(template, new { app = app ?? new AppViewModel(), issue = issue }, file);
        }

        public static void RemoveIssue(int issueId)
        {
            string folder = Path.Combine(ConfigService.RootPath, issueId.ToString());
            Directory.Delete(folder, true);
        }

        public static void RenderTemplateToFile(Template template, object data, string file)
        {
            string folder = Path.GetDirectoryName(file);
            Directory.CreateDirectory(folder);

            using (var writer = File.CreateText(file))
            {
                template.Render(data, writer, (p) => { p = Path.Combine(ConfigService.RootPath, p); if (!File.Exists(p)) { p += ".mustache"; } return LoadTemplate(p); });
            }
        }

        public static void PregenerateApp()
        {
            var vm = new { app = new AppViewModel() };
            var foos = new[] { new { template = "login.mustache", output = "login/index.html" },
                               new { template = "bugform.mustache", output = "new/index.html" } };
            foreach (var foo in foos)
            {
                Template template = LoadTemplate(foo.template);
                RenderTemplateToFile(template, vm, Path.Combine(ConfigService.RootPath, foo.output));
            }
        }
    }
}
