namespace RobMensching.TinyBugs.Views
{
    using System;
    using System.Net;
    using System.Web;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;

    public class TemplateView : StatusCodeView
    {
        public TemplateView(string path, object data = null, HttpStatusCode statusCode = HttpStatusCode.OK)
            : base(statusCode)
        {
            this.Path = path;
            this.Data = data;
        }

        public string Path { get; set; }

        public string ContentType { get; set; }

        public object Data { get; set; }

        public override void Execute(HttpContextBase context)
        {
            base.Execute(context);

            if (!String.IsNullOrEmpty(this.ContentType))
            {
                context.Response.ContentType = this.ContentType;
            }

            var template = FileService.LoadTemplate(this.Path);
            FileService.RenderTemplateToWriter(template, this.Data, context.Response.Output);
        }
    }
}
