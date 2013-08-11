namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
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
            string fullname = context.Form["fullname"];
            string email = context.Form["email"];
            string password = context.Form["password"];
            string verifyPassword = context.Form["verifypassword"];
            string username = context.Form["username"];

            // Default username to email.
            if (String.IsNullOrEmpty(username))
            {
                username = email;
            }

            var errors = VerifyData(email, password, verifyPassword, username);
            if (errors.Length > 0)
            {
                return new JsonView(errors, HttpStatusCode.BadRequest);
            }

            User user = UserService.Create(email, password, username);
            user.FullName = String.IsNullOrEmpty(fullname) ? null : fullname;
            user.VerifyToken = UserService.GenerateVerifyToken();

            using (var db = DataService.Connect())
            using (var tx = db.OpenTransaction())
            {
                db.Insert(user);
                user.Id = db.GetLastInsertId();

                // First user magically becomes an admin.
                if (user.Id == 1)
                {
                    user.Role = UserRole.Admin;
                    user.VerifyToken = null;
                    db.Update(user);
                }
                else
                {
                    MailService.SendVerifyUser(user.Email, user.VerifyToken);
                }

                tx.Commit();
            }

            return new JsonView(new UserViewModel(user), HttpStatusCode.Created);
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

            if (String.IsNullOrEmpty(password))
            {
                errors.Add(new ValidationError() { Field = "password", Message = "Password should be at least 5 characters." });
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
