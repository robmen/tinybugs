namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Linq;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public class UserApiController : ControllerBase
    {
        public override void Execute()
        {
            string query = this.Context.QueryString["q"] ?? String.Empty;
            using (var db = DataService.Connect(true))
            {
                var usernames = db.Select<User>(ev => ev.Select(u => u.UserName).Where(u => u.UserName.Contains(query))).Select(u => u.UserName);
                JsonSerializer.SerializeToWriter(usernames, this.Context.GetOutput("application/json"));
            }
        }
    }
}
