namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("api/user")]
    public class UsersApiController : ControllerBase
    {
        private static Regex UsernameValidation = new Regex("[A-Za-z0-9]{3,15}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public override ViewBase Get(ControllerContext context)
        {
            string query = context.QueryString["q"] ?? String.Empty;
            using (var db = DataService.Connect(true))
            {
                var usernames = db.Select<User>(ev => ev.Select(u => u.UserName).Where(u => u.UserName.Contains(query))).Select(u => u.UserName);
                return new JsonView(usernames);
            }
        }

        public override ViewBase Post(ControllerContext context)
        {
            string email = context.Form["email"];
            string password = context.Form["password"];
            string verifyPassword = context.Form["verifypassword"];
            string username = context.Form["username"];

            var errors = VerifyData(email, password, verifyPassword, username);
            if (errors.Length > 0)
            {
                return new JsonView(errors, HttpStatusCode.BadRequest);
            }

            User user = UserService.Create(email, password, username);
            user.VerifyToken = UserService.GenerateVerifyToken();

            using (var db = DataService.Connect())
            using (var tx = db.OpenTransaction())
            {
                db.Insert(user);
                MailService.SendVerifyUser(user.Email, user.VerifyToken);

                tx.Commit();
            }

            // TODO: create user view model and serialize that and send back the HttpStatusCode.Created.
            //JsonSerializer.SerializeToWriter(vm, context.GetOutput("application/json"));
            return new StatusCodeView(HttpStatusCode.Created);
        }

        private ValidationError[] VerifyData(string email, string password, string verifyPassword, string username)
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (String.IsNullOrEmpty(email))
            {
                errors.Add(new ValidationError() { Field = "email", Message = "Email address is required." });
            }
            else if (!email.Contains('@'))
            {
                errors.Add(new ValidationError() { Field = "email", Message = "Please provide valid email address." });
            }

            if (String.IsNullOrEmpty(password) || password.Length < 5)
            {
                errors.Add(new ValidationError() { Field = "password", Message = "Password must be at least 5 characters." });
            }
            else if (verifyPassword != password)
            {
                errors.Add(new ValidationError() { Field = "verifyPassword", Message = "Passwords must match." });
            }

            if (String.IsNullOrEmpty(username) || !UsernameValidation.IsMatch(username))
            {
                errors.Add(new ValidationError() { Field = "username", Message = "Usernames must be three to fifteen characters long and can only contain letters and numbers." });
            }

            return errors.ToArray();
        }
    }
}
