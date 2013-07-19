namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Generic;
    using ServiceStack.DataAnnotations;

    public class Issue
    {
        public Issue()
        {
            this.Tags = new List<string>();
        }

        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public Guid AssignedToId { get; set; }

        [References(typeof(User))]
        public Guid CreatedById { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public IssueStatus Status { get; set; }

        public string Resolution { get; set; }

        public string Release { get; set; }

        public List<string> Tags { get; private set; }

        public string Text { get; set; }

        public string Title { get; set; }

        public IssueType Type { get; set; }

        public bool Private { get; set; }

        public int Votes { get; set; }
    }
}
