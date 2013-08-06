namespace RobMensching.TinyWebStack
{
    using System.Web;

    public abstract class ViewBase
    {
        public abstract void Execute(HttpContextBase context);
    }
}
