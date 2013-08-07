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
                string assetPath = Path.Combine("assets", path);

                template = new Template();
                using (var reader = FileService.ReadFile(assetPath))
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

        public static void WriteIssue(IssueViewModel issue, BreadcrumbsViewModel breadcrumbs, AppViewModel app = null)
        {
            string file = Path.Combine(ConfigService.RootPath, issue.Id.ToString() + "\\index.html");

            Template template = LoadTemplate("bug.mustache");
            RenderTemplateToFile(template, new { app = app ?? new AppViewModel(), breadcrumbs = breadcrumbs, issue = issue }, file);
        }

        public static void RemoveIssue(long issueId)
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
                RenderTemplateToWriter(template, data, writer);
            }
        }

        public static void RenderTemplateToWriter(Template template, object data, TextWriter writer)
        {
            template.Render(data, writer, (p) => { return LoadTemplate(p); });
        }

        public static void PregenerateApp()
        {
            var vm = new { app = new AppViewModel(), breadcrumbs = ConfigService.ExternalBreadcrumbs };
            var foos = new[] { new { template = "tinybugs.js.mustache", output = "assets/js/tinybugs.js" },
                               new { template = "admin.mustache", output = "admin/index.html" },
                               new { template = "login.mustache", output = "login/index.html" },
                               new { template = "login_create.mustache", output = "login/create/index.html" },
                               new { template = "login_verify.mustache", output = "login/verify/index.html" },
                               new { template = "bugform.mustache", output = "new/index.html" }, };
            foreach (var foo in foos)
            {
                Template template = LoadTemplate(foo.template);
                RenderTemplateToFile(template, vm, Path.Combine(ConfigService.RootPath, foo.output));
            }
        }
    }
}
