namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Generic;
    using ServiceStack.DataAnnotations;

    public class IssueUpdate
    {
        public IssueUpdate()
        {
            this.Changes = new List<IssueChange>();
        }

        [AutoIncrement]
        public long Id { get; set; }

        [References(typeof(Issue))]
        public long IssueId { get; set; }

        [References(typeof(User))]
        public Guid UpdateByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<IssueChange> Changes { get; set; }

        public string Text { get; set; }
    }
}
