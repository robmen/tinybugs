namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.DataAnnotations;

    public class User : IIdentity, IPrincipal
    {
        public static Regex UsernameValidation = new Regex("^[A-Za-z0-9]{3,15}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [AutoIncrement]
        public long Id { get; set; }

        [Index(Unique = true)]
        public Guid Guid { get; set; }

        [Index(Unique = true)]
        public string Email { get; set; }

        [Index(Unique = true)]
        public string UserName { get; set; }

        public UserRole Role { get; set; }

        public string FullName { get; set; }

        public string GravatarId { get; set; }

        public string Salt { get; set; }

        public string PasswordHash { get; set; }

        public string VerifyToken { get; set; }

        [Ignore]
        public string AuthenticationType
        {
            get { return "Custom"; }
        }

        [Ignore]
        public bool IsAuthenticated
        {
            get { return this.Guid != Guid.Empty; }
        }

        [Ignore]
        public string Name
        {
            get { return this.Guid.ToString(); }
        }

        [Ignore]
        public IIdentity Identity
        {
            get { return this; }
        }

        public bool IsInRole(string role)
        {
            UserRole userRole;
            if (Enum.TryParse(role, true, out userRole))
            {
                return this.IsInRole(userRole);
            }

            return false;
        }

        public bool IsInRole(UserRole userRole)
        {
            return userRole <= this.Role;
        }

        public PopulateResults PopulateWithData(NameValueCollection data, User user, bool checkRequired = false)
        {
            PopulateResults results = new PopulateResults();
            string username = null;

            foreach (string name in data.AllKeys)
            {
                string[] values = data.GetValues(name);
                string value = values[values.Length - 1].Trim();
                switch (name.ToLowerInvariant())
                {
                    case "email":
                        {
                            string email = value.ToLowerInvariant();
                            if (email != this.Email)
                            {
                                string gravatar = UserService.GenerateGravatarId(email);
                                string verifyToken = UserService.GenerateVerifyToken();

                                results.Updates.Add("Email", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.Email,
                                    New = this.Email = email,
                                });

                                results.Updates.Add("GravatarId", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.GravatarId,
                                    New = this.GravatarId = gravatar,
                                });

                                results.Updates.Add("VerifyToken", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.VerifyToken,
                                    New = this.VerifyToken = verifyToken,
                                });
                            }
                        }
                        break;

                    case "fullname":
                        if (value != this.FullName)
                        {
                            results.Updates.Add("FullName", new PopulateResults.UpdatedValue()
                            {
                                Old = this.FullName,
                                New = this.FullName = value,
                            });
                        }
                        break;

                    case "username":
                        if (String.IsNullOrEmpty(value))
                        {
                            username = String.Empty;
                        }
                        else if (UsernameValidation.IsMatch(value))
                        {
                            username = value;
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = "username", Message = "Usernames must be three to fifteen characters long and can only contain letters and numbers." });
                        }
                        break;

                    case "role":
                        if (user.IsInRole(UserRole.Admin))
                        {
                            UserRole role;
                            if (Enum.TryParse(value, true, out role))
                            {
                                if (role != this.Role)
                                {
                                    results.Updates.Add("Role", new PopulateResults.UpdatedValue()
                                    {
                                        Old = this.Role,
                                        New = this.Role = role,
                                    });
                                }
                            }
                            else
                            {
                                results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown user role." });
                            }
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Forbidden." });
                        }
                        break;
                }
            }

            // Check username last since it might default to an updated email.
            if (username != null)
            {
                if (String.IsNullOrEmpty(username))
                {
                    username = this.Email;
                }

                else if (username != this.UserName)
                {
                    results.Updates.Add("UserName", new PopulateResults.UpdatedValue()
                    {
                        Old = this.UserName,
                        New = this.UserName = username,
                    });
                }
            }

            if (checkRequired)
            {
                if (String.IsNullOrEmpty(this.Email))
                {
                    results.Errors.Add(new ValidationError() { Field = "email", Message = "Required." });
                }
            }

            return results;
        }
    }
}
