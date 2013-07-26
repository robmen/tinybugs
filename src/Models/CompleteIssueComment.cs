namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Generic;
    using ServiceStack.DataAnnotations;

    public class CompleteIssueComment
    {
        [BelongTo(typeof(IssueComment))]
        public long Id { get; set; }

        [BelongTo(typeof(User))]
        public string CommentByUserEmail { get; set; }

        [BelongTo(typeof(User))]
        public string CommentByUserName { get; set; }

        [BelongTo(typeof(IssueComment))]
        public DateTime CreatedAt { get; set; }

        [BelongTo(typeof(IssueComment))]
        public List<IssueChange> Changes { get; set; }

        [BelongTo(typeof(IssueComment))]
        public string Text { get; set; }
    }
}
