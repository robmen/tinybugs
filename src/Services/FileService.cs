namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Resources;
    using System.Reflection;
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

                string assetPath = Path.Combine(ConfigService.RootPath, "assets\\custom", path);
                if (File.Exists(assetPath))
                {
                    using (var reader = FileService.ReadFile(assetPath))
                    {
                        template.Load(reader);
                    }
                }
                else
                {
                    string assetStream = String.Concat("RobMensching.TinyBugs.assets.", path.Replace('/', '.').Replace('\\', '.'));
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assetStream))
                    {
                        StreamReader s = new StreamReader(stream);
                        template.Load(s);
                    }
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
            var vm = new { app = new AppViewModel(), breadcrumbs = ConfigService.ExternalBreadcrumbs, mail = ConfigService.Mail };
            var pregens = new[] { new { template = "accessdenied.mustache", output = "accessdenied/index.html" },
                                  new { template = "admin.mustache", output = "admin/index.html" },
                                  new { template = "login.mustache", output = "login/index.html" },
                                  new { template = "login_create.mustache", output = "login/create/index.html" },
                                  new { template = "login_reset.mustache", output = "login/reset/index.html" },
                                  new { template = "login_activate.mustache", output = "login/activate/index.html" },
                                  new { template = "bugform.mustache", output = "new/index.html" },
                                  new { template = "tinybugs.css.mustache", output = "assets/css/tinybugs.css" },
                                  new { template = "tinybugs.js.mustache", output = "assets/js/tinybugs.js" }, };
            foreach (var pregen in pregens)
            {
                Template template = LoadTemplate(pregen.template);
                RenderTemplateToFile(template, vm, Path.Combine(ConfigService.RootPath, pregen.output));
            }

            ExtractMarkdownEditor();
        }

        private static void ExtractMarkdownEditor()
        {
            string outputFolder = Path.Combine(ConfigService.RootPath, "assets/mdd");
            Directory.CreateDirectory(outputFolder);

            var files = new[] { "markdowndeeplib.min.js",
                               "mdd_ajax_loader.gif",
                               "mdd_gripper.png",
                               "mdd_help.htm",
                               "mdd_modal_background.png",
                               "mdd_styles.css",
                               "mdd_toolbar.png", };
            foreach (var file in files)
            {
                string outputPath = Path.Combine(ConfigService.RootPath, "assets\\mdd", file);
                if (!File.Exists(outputFolder))
                {
                    string streamName = String.Concat("RobMensching.TinyBugs.assets.mdd.", file);

                    using (var input = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamName))
                    using (var output = File.Create(outputPath))
                    {
                        int read = 0;
                        byte[] buffer = new byte[1024];
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
    }
}
