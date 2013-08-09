namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text.RegularExpressions;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.DataAnnotations;
    using ServiceStack.OrmLite;

    public class Issue
    {
        private static Regex ResolutionValidation = new Regex("[A-Za-z0-9 -_]{2,30}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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

        public PopulateResults PopulateWithData(NameValueCollection data, Guid userId, bool checkRequired = false)
        {
            PopulateResults results = new PopulateResults();

            foreach (string name in data.AllKeys)
            {
                string[] values = data.GetValues(name);
                string value = values[values.Length - 1].Trim();
                switch (name.ToLowerInvariant())
                {
                    case "assignedto":
                        {
                            User user = null;
                            if (String.IsNullOrEmpty(value) || QueryService.TryGetUser(userId, value, out user))
                            {
                                this.AssignedToUserId = (user != null) ? user.Id : Guid.Empty;
                                results.Updates.Add("AssignedToUserId", this.AssignedToUserId);
                            }
                            else
                            {
                                results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown username." });
                            }
                        }
                        break;

                    case "private":
                        this.Private = true; // this field is always tracked as results.Updates below.
                        break;

                    case "area":
                        this.Area = ConfigService.Areas.Where(a => a.Equals(value, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (String.IsNullOrEmpty(value) || !String.IsNullOrEmpty(this.Area))
                        {
                            results.Updates.Add("Area", this.Area);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown area." });
                        }
                        break;

                    case "release":
                        this.Release = ConfigService.Releases.Where(r => r.Equals(value, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (String.IsNullOrEmpty(value) || !String.IsNullOrEmpty(this.Release))
                        {
                            results.Updates.Add("Release", this.Release);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown release." });
                        }
                        break;

                    case "resolution":
                        if (String.IsNullOrEmpty(value) || ResolutionValidation.IsMatch(value))
                        {
                            this.Resolution = value;
                            results.Updates.Add("Resolution", this.Resolution);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Resolution must be 2 to 30 characters long and contain only spaces, underscores, dashes or alphanumerics." });
                        }
                        break;

                    case "status":
                        IssueStatus status;
                        if (Enum.TryParse(value, true, out status))
                        {
                            this.Status = status;
                            results.Updates.Add("Status", this.Status);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown issue status." });
                        }
                        break;

                    //case "tag":
                    //case "tags":
                    //    this.Tags.Clear();
                    //    this.Tags.AddRange(values);
                    //    results.Updates.Add("Tags", this.Tags);
                    //    break;

                    case "text":
                        if (!String.IsNullOrEmpty(value))
                        {
                            this.Text = value;
                            results.Updates.Add("Text", this.Text);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Required." });
                        }
                        break;

                    case "title":
                        if (!String.IsNullOrEmpty(value))
                        {
                            this.Title = value;
                            results.Updates.Add("Title", this.Title);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Required." });
                        }
                        break;

                    case "type":
                        IssueType type;
                        if (Enum.TryParse(value, true, out type))
                        {
                            this.Type = type;
                            results.Updates.Add("Type", this.Type);
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown issue type." });
                        }
                        break;

                    //case "votes":
                    //    int votes;
                    //    if (Int32.TryParse(value, out votes))
                    //    {
                    //        this.Votes = votes;
                    //        results.Updates.Add("Votes", this.Votes);
                    //    }
                    //    break;

                    default:
                        results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown field." });
                        break;
                }
            }

            if (checkRequired)
            {
                if (String.IsNullOrEmpty(this.Title))
                {
                    results.Errors.Add(new ValidationError() { Field = "title", Message = "Required." });
                }

                if (String.IsNullOrEmpty(this.Text))
                {
                    results.Errors.Add(new ValidationError() { Field = "text", Message = "Required." });
                }
            }

            // Always update the boolean since we don't know what state it was
            // in originally.
            results.Updates.Add("Private", this.Private);

            this.UpdatedAt = DateTime.UtcNow;
            results.Updates.Add("results.UpdatesAt", this.UpdatedAt);

            return results;
        }
    }
}
