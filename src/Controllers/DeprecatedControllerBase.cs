namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Routing;

    public abstract class DeprecatedControllerBase : IHttpHandler
    {
        public DeprecatedControllerContext Context { get; private set; }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        public virtual bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            this.Context = new DeprecatedControllerContext(context);
            this.Execute();
        }

        public abstract void Execute();
    }
}
