namespace RobMensching.TinyBugs.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public class ConfigApiController : ControllerBase
    {
        public override void Execute()
        {
            switch (this.Context.Method)
            {
                case "GET":
                    JsonSerializer.SerializeToWriter(new AppViewModel(), this.Context.GetOutput("application/json"));
                    break;

                case "POST":
                    if (!this.Context.Authenticated)
                    {
                        this.Context.SetStatusCode(HttpStatusCode.BadGateway); // TODO: return a better error code that doesn't cause forms authentication to overwrite our response
                    }
                    else
                    {
                        using (var db = DataService.Connect(true))
                        {
                            User me = db.GetById<User>(this.Context.User);
                            if (me.Role < UserRole.Admin)
                            {
                                this.Context.SetStatusCode(HttpStatusCode.Forbidden);
                                break;
                            }
                        }

                        var config = new Config();
                        var updates = config.PopulateWithData(this.Context.Form);

                        using (var db = DataService.Connect())
                        {
                            db.Insert(config);
                        }

                        ConfigService.InitializeWithConfig(config);
                        FileService.PregenerateApp();

                        JsonSerializer.SerializeToWriter(new AppViewModel(), this.Context.GetOutput("application/json"));
                    }
                    break;

                default:
                    this.Context.SetStatusCode(HttpStatusCode.MethodNotAllowed);
                    break;
            }
        }
    }
}
