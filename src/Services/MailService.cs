namespace RobMensching.TinyBugs.Services
{
    using System.Net;
    using System.Net.Mail;
    using Nustache.Core;
    using RobMensching.TinyBugs.ViewModels;

    public class MailService
    {
        private const string Subject = "{{app.name}} Password Assistance";
        private const string Body = 
@"We received a request to reset the password associated with this e-mail address. If you made this request, please follow the instructions below.

Click the link below to reset your password using our secure server:

{{app.url}}login/verify?t={{token}}

If you did not request to have your password reset you can safely ignore this email. Rest assured your account is safe. 

If clicking the link doesn't seem to work, you can copy and paste the link into your browser's address window, or retype it there. Once you have returned to {{app.name}}, we will give instructions for resetting your password.";

        public static void SendVerifyUser(string email, string token)
        {
            var vm = new { app = new AppViewModel(), email = email, token = token };
            string subject = Render.StringToString(Subject, vm);
            string body = Render.StringToString(Body, vm);

            using (var smtp = new SmtpClient("mail.robmensching.com", 587))
            using (var message = new MailMessage(ConfigService.FromEmail, email))
            {
                smtp.EnableSsl = false;
                smtp.Credentials = new NetworkCredential("test@robmensching.com", "test");

                message.Subject = subject;
                message.Body = body;

                smtp.Send(message);
            }
        }
    }
}
