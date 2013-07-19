namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using RobMensching.TinyBugs.Models;

    public static class UserService
    {
        private const int PasswordHashIterations = 20000;
        private static readonly char[] TrimEquals = new char[] { '=' };

        public static User Create(string email, string password = null)
        {
            byte[] salt = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return new User()
            {
                Id = Guid.NewGuid(),
                Email = email.ToLowerInvariant(),
                Salt = Convert.ToBase64String(salt).TrimEnd(TrimEquals),
                PasswordHash = CalculatePasswordHash(email, salt, password),
            };
        }

        public static string CalculatePasswordHash(string email, string salt, string password)
        {
            byte[] saltBytes = Convert.FromBase64String(salt.TrimEnd(TrimEquals));
            return CalculatePasswordHash(email, saltBytes, password);
        }

        public static string CalculatePasswordHash(string email, byte[] salt, string password)
        {
            byte[] passwordBytes;
            using (SHA256 sha = SHA256.Create())
            {
                string emailPassword = email.ToLowerInvariant() + password;
                passwordBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(emailPassword));
            }

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, PasswordHashIterations))
            {
                byte[] bytes = pbkdf2.GetBytes(64);
                return Convert.ToBase64String(bytes).TrimEnd(TrimEquals);
            }
        }
    }
}
