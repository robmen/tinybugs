namespace RobMensching.TinyBugs.Controllers
{
    using System.Web;

    public abstract class ControllerBase : IHttpHandler
    {
        public ControllerContext Context { get; private set; }

        public virtual bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            this.Context = new ControllerContext(context);
            this.Execute();
        }

        public abstract void Execute();
    }
}
