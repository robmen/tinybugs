namespace RobMensching.TinyBugs.Controllers
{
    using System.Net;
    using System.Web.Routing;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using RobMensching.TinyWebStack;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    [Route("api/config")]
    public class ConfigApiController : ControllerBase
    {
        public override ViewBase Get(ControllerContext context)
        {
            JsonSerializer.SerializeToWriter(new AppViewModel(), context.GetOutput("application/json"));
            return null;
        }

        //[Authenticate(Forms, "/login/")] // Basic, Digest, Oauth, Api
        public override ViewBase Post(ControllerContext context)
        {
            if (!context.Authenticated)
            {
                context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }
            else
            {
                using (var db = DataService.Connect(true))
                {
                    User me = db.GetById<User>(context.User);
                    if (me.Role < UserRole.Admin)
                    {
                        context.SetStatusCode(HttpStatusCode.Forbidden);
                        return null;
                    }
                }

                var config = new Config();
                var updates = config.PopulateWithData(context.Form);

                using (var db = DataService.Connect())
                {
                    db.Insert(config);
                }

                ConfigService.InitializeWithConfig(config);
                FileService.PregenerateApp();

                JsonSerializer.SerializeToWriter(new AppViewModel(), context.GetOutput("application/json"));
            }

            return null;
        }
    }
}
