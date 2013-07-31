namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using RobMensching.TinyBugs.Models;
    using ServiceStack.OrmLite;

    public static class UserService
    {
        private const int PasswordHashIterations = 20000;

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
                UserName = email.ToLowerInvariant(),
                GravatarId = GenerateGravatarId(email),
                Salt = Convert.ToBase64String(salt),
                PasswordHash = CalculatePasswordHash(email, salt, password),
            };
        }

        public static string CalculatePasswordHash(string username, string salt, string password)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            return CalculatePasswordHash(username, saltBytes, password);
        }

        public static string CalculatePasswordHash(string username, byte[] salt, string password)
        {
            byte[] passwordBytes;
            using (SHA256 sha = SHA256.Create())
            {
                string namePassword = username.Trim().ToLowerInvariant() + password;
                passwordBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(namePassword));
            }

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, PasswordHashIterations))
            {
                byte[] bytes = pbkdf2.GetBytes(64);
                return Convert.ToBase64String(bytes);
            }
        }

        public static string GenerateGravatarId(string email)
        {
            byte[] emailBytes = Encoding.ASCII.GetBytes(email.Trim().ToLowerInvariant());
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] idBytes = md5.ComputeHash(emailBytes);
                return BytesToHex(idBytes);
            }
        }

        public static string GetGravatarImageUrl(string id, bool secure = false, int size = 0)
        {
            string protocol = secure ? "https" : "http";
            string sizeQuery = size > 0 ? "&s=" + size : String.Empty;
            return String.Format("{0}://www.gravatar.com/avatar/{1}?r=g&d=mm{2}", protocol, id, sizeQuery);
        }

        public static string GetGravatarImageUrlForEmail(string email, bool secure = false, int size = 0)
        {
            string id = GenerateGravatarId(email);
            string protocol = secure ? "https" : "http";
            string sizeQuery = size > 0 ? "&s=" + size : String.Empty;
            return String.Format("{0}://www.gravatar.com/avatar/{1}?r=g&d=mm{2}", protocol, id, sizeQuery);
        }

        public static bool TryAuthenticateUser(string username, string password, out User user)
        {
            username = String.IsNullOrEmpty(username) ? String.Empty : username.ToLowerInvariant();

            using (var db = DataService.Connect())
            {
                user = db.SelectParam<User>(u => u.UserName == username).SingleOrDefault();
                if (user == null)
                {
                    return false;
                }
            }

            string hash = UserService.CalculatePasswordHash(user.UserName, user.Salt, password);
            return user.PasswordHash.Equals(hash, StringComparison.Ordinal);
        }

        private static string BytesToHex(byte[] bytes)
        {
            const string hexAlphabet = "0123456789abcdef";

            StringBuilder sb = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
            {
                sb.Append(hexAlphabet[(int)(b >> 4)]);
                sb.Append(hexAlphabet[(int)(b & 0xF)]);
            }

            return sb.ToString();
        }
    }
}
