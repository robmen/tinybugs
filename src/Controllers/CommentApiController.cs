namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RobMensching.TinyWebStack;

    [Route("api/comment")]
    public class CommentApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            context.GetOutput().Write("<html><head><title>Comments</title></head><body><h1>Comments</h1></body></html>");
            return null;
        }
    }
}
