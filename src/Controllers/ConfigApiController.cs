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
                return new StatusCodeView(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
            }
            else
            {
                using (var db = DataService.Connect(true))
                {
                    User me = db.GetById<User>(context.User);
                    if (me.Role < UserRole.Admin)
                    {
                        return new StatusCodeView(HttpStatusCode.Forbidden);
                    }
                }

                var config = new Config();
                var updates = config.PopulateWithData(context.Form);

                using (var db = DataService.Connect())
                using (var tx = db.OpenTransaction())
                {
                    db.Insert(config);

                    ConfigService.InitializeWithConfig(config);
                    FileService.PregenerateApp();

                    tx.Commit();
                }

                return new JsonView(new AppViewModel());
            }
        }
    }
}
