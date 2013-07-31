namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Nustache.Core;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.ViewModels;

    public static class FileService
    {
        private static ConcurrentDictionary<string, Template> CompiledTemplates = new ConcurrentDictionary<string, Template>();

        public static string RootPath { get; set; }

        public static void Initialize(string root)
        {
            RootPath = root;
        }

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
            string fullPath = Path.Combine(RootPath, path);
            return File.OpenText(fullPath);
        }

        public static void WriteIssue(AppViewModel app, CompleteIssue issue)
        {
            string file = Path.Combine(RootPath, issue.Id.ToString() + "\\index.html");

            Template template = LoadTemplate("bug.mustache");
            RenderTemplateToFile(template, new { app = app, issue = issue }, file);
        }

        public static void RemoveIssue(int issueId)
        {
            string folder = Path.Combine(RootPath, issueId.ToString());
            Directory.Delete(folder, true);
        }

        public static void RenderTemplateToFile(Template template, object data, string file)
        {
            string folder = Path.GetDirectoryName(file);
            Directory.CreateDirectory(folder);

            using (var writer = File.CreateText(file))
            {
                template.Render(data, writer, (p) => { p = Path.Combine(RootPath, p); if (!File.Exists(p)) { p += ".mustache"; } return LoadTemplate(p); });
            }
        }

        public static void PregenerateApp(AppViewModel app)
        {
            var vm = new { app = app };
            var foos = new[] { new { template = "login.mustache", output = "login/index.html" },
                               new { template = "bugform.mustache", output = "new/index.html" } };
            foreach (var foo in foos)
            {
                Template template = LoadTemplate(foo.template);
                RenderTemplateToFile(template, vm, Path.Combine(RootPath, foo.output));
            }
        }
    }
}
