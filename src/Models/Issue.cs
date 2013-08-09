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
            bool privateIssue = false;

            foreach (string name in data.AllKeys)
            {
                string[] values = data.GetValues(name);
                string value = values[values.Length - 1].Trim();
                switch (name.ToLowerInvariant())
                {
                    case "assignedto":
                        {
                            User newUser = null;
                            if (String.IsNullOrEmpty(value) || QueryService.TryGetUser(userId, value, out newUser))
                            {
                                Guid assignedTo = (newUser != null) ? newUser.Id : Guid.Empty;
                                if (assignedTo != this.AssignedToUserId)
                                {
                                    User oldUser;
                                    using (var db = DataService.Connect(true))
                                    {
                                        oldUser = db.GetByIdOrDefault<User>(this.AssignedToUserId);
                                    }

                                    results.Updates.Add("AssignedToUserId", new PopulateResults.UpdatedValue()
                                    {
                                        FriendlyName = "AssignedTo",
                                        FriendlyOld = oldUser == null ? String.Empty : oldUser.UserName,
                                        FriendlyNew = newUser == null ? String.Empty : newUser.UserName,
                                        Old = this.AssignedToUserId,
                                        New = this.AssignedToUserId = assignedTo,
                                    });
                                }
                            }
                            else
                            {
                                results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown username." });
                            }
                        }
                        break;

                    case "private":
                        privateIssue = true; // this field change is tracked  below.
                        break;

                    case "area":
                        {
                            string area = ConfigService.Areas.Where(a => a.Equals(value, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (String.IsNullOrEmpty(value) || !String.IsNullOrEmpty(area))
                            {
                                if ((area ?? String.Empty) != (this.Area ?? String.Empty))
                                {
                                    results.Updates.Add("Area", new PopulateResults.UpdatedValue()
                                    {
                                        Old = this.Area,
                                        New = this.Area = area,
                                    });
                                }
                            }
                            else
                            {
                                results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown area." });
                            }
                        }
                        break;

                    case "release":
                        {
                            string release = ConfigService.Releases.Where(r => r.Equals(value, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (String.IsNullOrEmpty(value) || !String.IsNullOrEmpty(release))
                            {
                                if ((release ?? String.Empty) != (this.Release ?? String.Empty))
                                {
                                    results.Updates.Add("Release", new PopulateResults.UpdatedValue()
                                    {
                                        Old = this.Release,
                                        New = this.Release = release,
                                    });
                                }
                            }
                            else
                            {
                                results.Errors.Add(new ValidationError() { Field = name, Message = "Unknown release." });
                            }
                        }
                        break;

                    case "resolution":
                        if (String.IsNullOrEmpty(value) || ResolutionValidation.IsMatch(value))
                        {
                            if ((value ?? String.Empty) != (this.Resolution ?? String.Empty))
                            {
                                results.Updates.Add("Resolution", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.Resolution,
                                    New = this.Resolution = value,
                                });
                            }
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
                            if (status != this.Status)
                            {
                                results.Updates.Add("Status", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.Status,
                                    New = this.Status = status,
                                });
                            }
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
                            if ((value ?? String.Empty) != (this.Text ?? String.Empty))
                            {
                                results.Updates.Add("Text", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.Text,
                                    New = this.Text = value,
                                });
                            }
                        }
                        else
                        {
                            results.Errors.Add(new ValidationError() { Field = name, Message = "Required." });
                        }
                        break;

                    case "title":
                        if (!String.IsNullOrEmpty(value))
                        {
                            if ((value ?? String.Empty) != (this.Title ?? String.Empty))
                            {
                                results.Updates.Add("Title", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.Title,
                                    New = this.Title = value,
                                });
                            }
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
                            if (type != this.Type)
                            {
                                results.Updates.Add("Type", new PopulateResults.UpdatedValue()
                                {
                                    Old = this.Type,
                                    New = this.Type = type,
                                });
                            }
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
            if (privateIssue != this.Private)
            {
                results.Updates.Add("Private", new PopulateResults.UpdatedValue()
                {
                    Old = this.Private,
                    New = this.Private = privateIssue,
                });
            }

            results.Updates.Add("UpdatedAt", new PopulateResults.UpdatedValue()
            {
                Old = this.UpdatedAt,
                New = this.UpdatedAt = DateTime.UtcNow,
            });
 
            return results;
        }
    }
}
