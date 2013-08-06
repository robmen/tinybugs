namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    [Route("api/user")]
    public class UsersApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            string query = context.QueryString["q"] ?? String.Empty;
            using (var db = DataService.Connect(true))
            {
                var usernames = db.Select<User>(ev => ev.Select(u => u.UserName).Where(u => u.UserName.Contains(query))).Select(u => u.UserName);
                JsonSerializer.SerializeToWriter(usernames, context.GetOutput("application/json"));
            }

            return null;
        }

        public override ViewBase Post(ControllerContext context)
        {
            string email = context.Form["email"];
            string password = context.Form["password"];
            string verifyPassword = context.Form["verifypassword"];

            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || password != verifyPassword)
            {
                context.SetStatusCode(HttpStatusCode.BadRequest);
                return null;
            }

            string username = context.Form["username"];

            // TODO: verify input

            User user = UserService.Create(email, password, username);
            user.VerifyToken = UserService.GenerateVerifyToken();

            using (var db = DataService.Connect())
            using (var tx = db.OpenTransaction())
            {
                db.Insert(user);
                MailService.SendVerifyUser(user.Email, user.VerifyToken);

                tx.Commit();
            }

            context.SetStatusCode(HttpStatusCode.Created);

            // TODO: create user view model and serialize that.
            //JsonSerializer.SerializeToWriter(vm, context.GetOutput("application/json"));
            return null;
        }
    }
}
