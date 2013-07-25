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
            IssuesPaged issuesPaged = QueryService.QueryIssues(q);
            var pagePrefix = this.Context.ApplicationPath + "api/" + QueryService.RecreateQueryString(q) + "&page=";

            ApiViewModel vm = new ApiViewModel()
            {
                Issues = issuesPaged.Issues,
                Page = issuesPaged.Page,
                Pages = issuesPaged.Pages,
                Total = issuesPaged.Total,
                PageUriPrefix = pagePrefix,
                PreviousPageUri = (issuesPaged.PreviousPage > 0) ? pagePrefix + issuesPaged.PreviousPage : null,
                NextPageUri = (issuesPaged.NextPage > 0) ? pagePrefix + issuesPaged.NextPage : null,
            };

            JsonSerializer.SerializeToWriter(vm, this.Context.GetOutput("application/json"));
        }
    }
}
