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
            string handlerPath = this.Context.ApplicationPath + "api/user/";
#if DEBUG
            if (!this.Context.Url.AbsolutePath.WithTrailingSlash().StartsWithIgnoreCase(handlerPath))
            {
                throw new ApplicationException(String.Format("Paths are out of sync. Expected URL path to start with {0} but found {1}", handlerPath, this.Context.Url.AbsolutePath));
            }
#endif
            string username = this.Context.Url.AbsolutePath.WithTrailingSlash().Substring(handlerPath.Length).TrimEnd('/');

            if (String.IsNullOrEmpty(username))
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
                        {
                            string email = this.Context.Form["email"];
                            string password = this.Context.Form["password"];
                            string verifyPassword = this.Context.Form["verifypassword"];

                            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || password != verifyPassword)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.BadRequest);
                                break;
                            }

                            username = this.Context.Form["username"];

                            User user = UserService.Create(email, password, username);
                            user.VerifyToken = UserService.GenerateVerifyToken();

                            using (var db = DataService.Connect())
                            using (var tx = db.OpenTransaction())
                            {
                                db.Insert(user);
                                MailService.SendVerifyUser(user.Email, user.VerifyToken);

                                tx.Commit();
                            }

                            this.Context.SetStatusCode(HttpStatusCode.Created);

                            // TODO: create user view model and serialize that.
                            //JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                        }
                        break;

                    default:
                        this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                        break;
                }
            }
            else
            {
                switch (this.Context.Method)
                {
                    case "GET":
                        {
                            var user = QueryService.GetUserFromName(username);
                            if (user == null)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.NotFound);
                            }
                            else
                            {
                                // TODO: create user view model and serialize that.
                                //JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
                            }
                        }
                        break;

                    case "POST":
                    case "PUT":
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

                            string formUsername = this.Context.Form["username"];
                            if (!username.Equals(formUsername))
                            {
                                this.Context.SetStatusCode(HttpStatusCode.BadRequest);
                                break;
                            }

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
}
