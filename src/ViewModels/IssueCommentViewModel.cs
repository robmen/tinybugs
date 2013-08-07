namespace RobMensching.TinyBugs.ViewModels
{
    using System;
    using System.Collections.Generic;
    using RobMensching.TinyBugs.Models;
    using ServiceStack.DataAnnotations;

    public class IssueCommentViewModel
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

        public RelativeDateViewModel CreatedRelative { get { return new RelativeDateViewModel(this.CreatedAt); } }

        public string TextRendered
        {
            get { return new MarkdownDeep.Markdown() { NoFollowLinks = true, ExtraMode = true, SafeMode = true }.Transform(this.Text); }
        }
    }
}
