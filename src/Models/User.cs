namespace RobMensching.TinyBugs.Models
{
    using System;
    using ServiceStack.DataAnnotations;

    public class User
    {
        public Guid Id { get; set; }

        [Index(Unique = true)]
        public string Email { get; set; }

        [Index(Unique = true)]
        public string UserName { get; set; }

        public string FullName { get; set; }

        public string GravatarId { get; set; }

        public string Salt { get; set; }

        public string PasswordHash { get; set; }

        public bool Verified { get; set; }
    }
}
