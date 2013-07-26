namespace RobMensching.TinyBugs.Controllers
{
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using RobMensching.TinyBugs.ViewModels;
    using ServiceStack.Text;

    public class ApiController : ControllerBase
    {
        public override void Execute()
        {
            Query q = QueryService.ParseQuery(this.Context.QueryString);
            QueriedIssues issuesPaged = QueryService.QueryIssues(q);
            var pagePrefix = this.Context.ApplicationPath + "api/" + QueryService.RecreateQueryString(q) + "&page=";

            ApiViewModel vm = new ApiViewModel()
            {
                Issues = issuesPaged.Issues,
                Page = new Pagination(q.Page, q.Count, issuesPaged.Total, pagePrefix),
            };

            JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
        }
    }
}
