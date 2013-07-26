namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CommentApiController : ControllerBase
    {
        public override void Execute()
        {
            this.Context.GetOutput().Write("<html><head><title>Comments</title></head><body><h1>Comments</h1></body></html>");
        }
    }
}
