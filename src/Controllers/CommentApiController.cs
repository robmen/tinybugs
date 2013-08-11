namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using RobMensching.TinyWebStack;

    [Route("api/comment")]
    public class CommentApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            return new StatusCodeView(HttpStatusCode.NotImplemented);
        }
    }
}
