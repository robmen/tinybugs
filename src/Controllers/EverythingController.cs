namespace RobMensching.TinyBugs.Controllers
{
    public class EverythingController : DeprecatedControllerBase
    {
        public override void Execute()
        {
            this.Context.GetOutput().Write("<html><head><title>Everything</title></head><body><h1>Everything</h1></body></html>");
        }
    }
}
