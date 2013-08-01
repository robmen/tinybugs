namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.DataAnnotations;
    using Microsoft.Security.Application;

    public class Issue
    {
        [AutoIncrement]
        public long Id { get; set; }

        [References(typeof(User))]
        public Guid AssignedToUserId { get; set; }

        [References(typeof(User))]
        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public IssueStatus Status { get; set; }

        public string Resolution { get; set; }

        public string Release { get; set; }

        public string Area { get; set; }

        public string Text { get; set; }

        public string Title { get; set; }

        public IssueType Type { get; set; }

        public bool Private { get; set; }

        public int Votes { get; set; }

        public Dictionary<string, object> PopulateWithData(NameValueCollection data)
        {
            Dictionary<string, object> updated = new Dictionary<string, object>();

            foreach (string name in data.AllKeys)
            {
                string[] values = data.GetValues(name);
                string unsafeValue = values[values.Length - 1];
                string value = unsafeValue; // Encoder.HtmlEncode(unsafeValue);
                switch (name.ToLowerInvariant())
                {
                    case "assigned":
                    case "assignedto":
                    case "assignedtouser":
                    case "assignedtoname":
                    case "assignedtousername":
                        this.AssignedToUserId = QueryService.GetUserFromName(value).Id;
                        if (this.AssignedToUserId != null)
                        {
                            updated.Add("AssignedToUserId", this.AssignedToUserId);
                        }
                        break;

                    case "assignedtoemail":
                        this.AssignedToUserId = QueryService.GetUserFromEmail(value).Id;
                        if (this.AssignedToUserId != null)
                        {
                            updated.Add("AssignedToUserId", this.AssignedToUserId);
                        }
                        break;

                    case "private":
                        bool priv;
                        if (Boolean.TryParse(value, out priv))
                        {
                            this.Private = priv;
                            updated.Add("Private", this.Private);
                        }
                        break;

                    case "area":
                        this.Area = value;
                        updated.Add("Area", this.Area);
                        break;

                    case "milestone":
                    case "release":
                        this.Release = value;
                        updated.Add("Release", this.Release);
                        break;

                    case "resolution":
                        this.Resolution = value;
                        updated.Add("Resolution", this.Resolution);
                        break;

                    case "status":
                        IssueStatus status;
                        if (Enum.TryParse(value, true, out status))
                        {
                            this.Status = status;
                            updated.Add("Status", this.Status);
                        }
                        break;

                    //case "tag":
                    //case "tags":
                    //    this.Tags.Clear();
                    //    this.Tags.AddRange(values);
                    //    updated.Add("Tags", this.Tags);
                    //    break;

                    case "content":
                    case "text":
                        {
                            this.Text = value;
                            updated.Add("Text", this.Text);
                        }
                        break;

                    case "title":
                        this.Title = value;
                        updated.Add("Title", this.Title);
                        break;

                    case "type":
                        IssueType type;
                        if (Enum.TryParse(value, true, out type))
                        {
                            this.Type = type;
                            updated.Add("Type", this.Type);
                        }
                        break;

                    case "vote":
                    case "votes":
                        int votes;
                        if (Int32.TryParse(value, out votes))
                        {
                            this.Votes = votes;
                            updated.Add("Votes", this.Votes);
                        }
                        break;
                }
            }

            this.UpdatedAt = DateTime.UtcNow;
            updated.Add("UpdatedAt", this.UpdatedAt);

            return updated;
        }
    }
}
