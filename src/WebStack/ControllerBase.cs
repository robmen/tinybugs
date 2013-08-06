namespace RobMensching.TinyWebStack
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Routing;

    public abstract class ControllerBase : IRouteHandler, IHttpHandler
    {
        private ControllerContext Context { get; set; }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var controller = (ControllerBase)Activator.CreateInstance(this.GetType());
            controller.Context = new ControllerContext(requestContext);
            return controller;
        }

        public virtual bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext http)
        {
            ViewBase view;

            var context = this.Context;
            switch (context.Method)
            {
                case "GET":
                    view = this.Get(context);
                    break;

                case "POST":
                    view = this.Post(context);
                    break;

                case "PUT":
                    view = this.Put(context);
                    break;

                case "DELETE":
                    view = this.Put(context);
                    break;

                case "PATCH":
                    view = this.Patch(context);
                    break;

                case "HEAD":
                    view = this.Head(context);
                    break;

                default:
                    view = this.UnknownMethod(context);
                    break;
            }

            if (view != null)
            {
                view.Execute(context.Context);
            }
        }

        public virtual ViewBase Get(ControllerContext context)
        {
            return this.UnknownMethod(context);
        }

        public virtual ViewBase Post(ControllerContext context)
        {
            return this.UnknownMethod(context);
        }

        public virtual ViewBase Put(ControllerContext context)
        {
            return this.UnknownMethod(context);
        }

        public virtual ViewBase Delete(ControllerContext context)
        {
            return this.UnknownMethod(context);
        }

        public virtual ViewBase Patch(ControllerContext context)
        {
            return this.UnknownMethod(context);
        }

        public virtual ViewBase Head(ControllerContext context)
        {
            return this.UnknownMethod(context);
        }

        public ViewBase UnknownMethod(ControllerContext context)
        {
            return new StatusCodeView(HttpStatusCode.MethodNotAllowed);
        }
    }
}
