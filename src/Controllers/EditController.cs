namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class EditController : ControllerBase
    {
        public override void Execute()
        {
            this.Context.GetOutput().Write("<html><head><title>Edit</title></head><body><h1>Edit</h1></body></html>");
        }
    }
}
