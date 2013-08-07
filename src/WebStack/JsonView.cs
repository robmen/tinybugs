namespace RobMensching.TinyWebStack
{
    using System.Net;
    using System.Web;
    using ServiceStack.Text;

    public class JsonView : StatusCodeView
    {
        public JsonView(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
            : base(statusCode)
        {
            this.ContentType = "application/json";
            this.Data = data;
        }

        public string ContentType { get; set; }

        public object Data { get; set; }

        public override void Execute(HttpContextBase context)
        {
            base.Execute(context);

            context.Response.ContentType = this.ContentType;
            JsonSerializer.SerializeToWriter(this.Data, context.Response.Output);
        }
    }
}
