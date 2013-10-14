namespace RobMensching.TinyBugs.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using Nustache.Core;
    using RobMensching.TinyBugs.ViewModels;

    public class MailService
    {
        private const string CommentSubject = "Updated {{app.name}} issue #{{issue.id}} - {{issue.title}}";
        private const string CommentBody =
@"Issue #{{issue.id}} - {{issue.title}}
  at: {{app.url}}{{issue.id}}/

{{#comment}}
{{#text}}Comment from:{{/text}}{{^text}}Edited by:{{/text}} {{commentbyusername}}
{{#changes}}
  * {{column}} {{#old}}changed from '{{old}}'{{/old}}{{^old}}set{{/old}} to {{#new}}'{{new}}'{{/new}}{{^new}}blank{{/new}}.
{{/changes}}

{{#text}}
{{{text}}}
{{/text}}
{{/comment}}";

        private const string PasswordResetSubject = "{{app.name}} Password Assistance";
        private const string PasswordResetBody =
@"We received a request to reset the password associated with this e-mail address. If you made this request, please follow the instructions below.

Click the link below to reset your password:

{{app.url}}login/reset?t={{token}}

If you did not request to have your password reset you can safely ignore this email. Rest assured your account is safe. 

If clicking the link doesn't seem to work, you can copy and paste the link into your browser's address window, or retype it there. Once you have returned to {{app.name}}, we will give instructions for resetting your password.";

        private const string VerifyUserSubject = "{{app.name}} Account Creation";
        private const string VerifyUserBody =
@"We received a request to create an account at {{app.name}}. If you made this request, please click the link below to activate your account:

{{app.url}}login/activate?t={{token}}

If you did not request a new account you can safely ignore this email.

If clicking the link doesn't seem to work, you can copy and paste the link into your browser's address window, or retype it there. Once you have returned to {{app.name}}, we will give instructions for activating your account.";

        public static void SendIssueComment(IssueViewModel issue, long commentId)
        {
            IssueCommentViewModel comment = issue.Comments.Where(c => c.Id == commentId).SingleOrDefault();
            var vm = new { app = new AppViewModel(), issue = issue, comment = comment };
            string subject = Render.StringToString(CommentSubject, vm);
            string body = Render.StringToString(CommentBody, vm);

            // Send to the opener of the issue if not the commenter.
            if (comment.CommentByUserId != issue.CreatedByUserId &&
                !String.IsNullOrEmpty(issue.CreatedByUserEmail))
            {
                Send(issue.CreatedByUserEmail, subject, body);
            }

            // Send to the holder of the issue if not also the commenter and opener.
            if (comment.CommentByUserId != issue.AssignedToUserId &&
                issue.AssignedToUserId != issue.CreatedByUserId &&
                !String.IsNullOrEmpty(issue.AssignedToUserEmail))
            {
                Send(issue.AssignedToUserEmail, subject, body);
            }
        }

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