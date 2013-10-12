namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;

    [Route("api/resend/{action}")]
    public class ResendApiController : ControllerBase
    {
        private enum ResendAction
        {
            Activate,
            Password,
        };

        public override ViewBase Post(ControllerContext context)
        {
            ResendAction action;
            string actionData = context.RouteData.Values["action"] as string;
            if (!Enum.TryParse<ResendAction>(actionData, true, out action))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            string email = context.Form["email"];
            if (String.IsNullOrEmpty(email))
            {
                return new StatusCodeView(HttpStatusCode.BadRequest);
            }

            return SendToken(context, action, email);
        }

        private ViewBase SendToken(ControllerContext context, ResendAction action, string nameOrEmail)
        {
            // If the name or email could not be found, pretend everything is okay.
            User user;
            if (!QueryService.TryGetUser(nameOrEmail, out user))
            {
                return null;
            }

            // Generate a verification token.
            user.VerifyToken = UserService.GenerateVerifyToken();

            using (var db = DataService.Connect())
            using (var tx = db.OpenTransaction())
            {
                db.UpdateOnly(user, u => u.VerifyToken, u => u.Guid == user.Guid);
                if (ResendAction.Activate == action)
                {
                    MailService.SendVerifyUser(user.Email, user.VerifyToken);
                }
                else
                {
                    MailService.SendPasswordReset(user.Email, user.VerifyToken);
                }

                tx.Commit();
            }

            return new StatusCodeView(HttpStatusCode.Created);
        }
    }
}
