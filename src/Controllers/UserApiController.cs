namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public class UserApiController : ControllerBase
    {
        public override void Execute()
        {
            switch (this.Context.Method)
            {
                case "GET":
                    {
                        string query = this.Context.QueryString["q"] ?? String.Empty;
                        using (var db = DataService.Connect(true))
                        {
                            var usernames = db.Select<User>(ev => ev.Select(u => u.UserName).Where(u => u.UserName.Contains(query))).Select(u => u.UserName);
                            JsonSerializer.SerializeToWriter(usernames, this.Context.GetOutput("application/json"));
                        }
                    }
                    break;

                case "POST":
                    if (!this.Context.Authenticated)
                    {
                        this.Context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
                    }
                    else
                    {
                        using (var db = DataService.Connect(true))
                        {
                            User me = db.GetById<User>(this.Context.User);
                            if (me.Role < UserRole.Admin)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.Forbidden);
                                break;
                            }
                        }

                        string username = this.Context.Form["username"];
                        User user = QueryService.GetUserFromName(username);
                        if (user == null)
                        {
                            this.Context.SetStatusCode(HttpStatusCode.NotFound);
                        }
                        else
                        {
                            UserRole role;
                            string newRole = this.Context.Form["role"];
                            if (Enum.TryParse<UserRole>(newRole, true, out role))
                            {
                                user.Role = role;
                                using (var db = DataService.Connect())
                                {
                                    db.UpdateOnly<User>(user, ev => ev.Update(u => u.Role).Where(u => u.Id == user.Id));
                                }

                                // TODO: create user view model and serialize that.
                                //JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                            }
                            else
                            {
                                this.Context.SetStatusCode(HttpStatusCode.BadRequest);
                            }
                        }
                    }
                    break;

                default:
                    this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                    break;
            }
        }
    }
}
