namespace RobMensching.TinyBugs.Controllers
{
    public class EverythingController : ControllerBase
    {
        public override void Execute()
        {
            this.Context.GetOutput().Write("<html><head><title>Everything</title></head><body><h1>Everything</h1></body></html>");
        }
    }
}
