namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Security.Principal;
    using ServiceStack.DataAnnotations;

    public class User : IIdentity, IPrincipal
    {
        public Guid Id { get; set; }

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
            get { return this.Id != Guid.Empty; }
        }

        [Ignore]
        public string Name
        {
            get { return this.Id.ToString(); }
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
    }
}
