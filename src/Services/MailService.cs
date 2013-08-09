namespace RobMensching.TinyBugs.Services
{
    using System.Net;
    using System.Net.Mail;
    using Nustache.Core;
    using RobMensching.TinyBugs.ViewModels;

    public class MailService
    {
        private const string PasswordResetSubject = "{{app.name}} Password Assistance";
        private const string PasswordResetBody =
@"We received a request to reset the password associated with this e-mail address. If you made this request, please follow the instructions below.

Click the link below to reset your password using our secure server:

{{app.url}}login/reset?t={{token}}

If you did not request to have your password reset you can safely ignore this email. Rest assured your account is safe. 

If clicking the link doesn't seem to work, you can copy and paste the link into your browser's address window, or retype it there. Once you have returned to {{app.name}}, we will give instructions for resetting your password.";

        private const string VerifyUserSubject = "{{app.name}} Password Assistance";
        private const string VerifyUserBody =
@"We received a request to reset the password associated with this e-mail address. If you made this request, please follow the instructions below.

Click the link below to reset your password using our secure server:

{{app.url}}login/verify?t={{token}}

If you did not request to have your password reset you can safely ignore this email. Rest assured your account is safe. 

If clicking the link doesn't seem to work, you can copy and paste the link into your browser's address window, or retype it there. Once you have returned to {{app.name}}, we will give instructions for resetting your password.";

        public static void SendPasswordReset(string email, string token)
        {
            var vm = new { app = new AppViewModel(), email = email, token = token };
            string subject = Render.StringToString(PasswordResetSubject, vm);
            string body = Render.StringToString(PasswordResetBody, vm);

            Send(email, subject, body);
        }

        public static void SendVerifyUser(string email, string token)
        {
            var vm = new { app = new AppViewModel(), email = email, token = token };
            string subject = Render.StringToString(VerifyUserSubject, vm);
            string body = Render.StringToString(VerifyUserBody, vm);

            Send(email, subject, body);
        }

        public static void Send(string email, string subject, string body)
        {
            using (var message = new MailMessage(ConfigService.Mail.From, email))
            {
                message.Subject = subject;
                message.Body = body;

                using (var smtp = new SmtpClient(ConfigService.Mail.Server, ConfigService.Mail.Port))
                {
                    smtp.EnableSsl = ConfigService.Mail.RequireSsl;
                    smtp.Credentials = new NetworkCredential(ConfigService.Mail.Username, ConfigService.Mail.Password);

                    smtp.Send(message);
                }
            }
        }
    }
}