namespace RobMensching.TinyBugs.Models
{
    public class MailConfig
    {
        public string From { get; set; }

        public string Server { get; set; }

        public int Port { get; set; }

        public bool RequireSsl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
