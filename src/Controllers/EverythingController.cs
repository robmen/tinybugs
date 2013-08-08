namespace RobMensching.TinyBugs.Controllers
{
    using RobMensching.TinyWebStack;

    //[Route("{*everything}")]
    public class EverythingController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            context.GetOutput().Write("<html><head><title>Everything</title></head><body><h1>Everything</h1></body></html>");
            return null;
        }
    }
}
