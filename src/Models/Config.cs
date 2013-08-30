namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using RobMensching.TinyBugs.Services;

    public class Config
    {
        public Config()
        {
            this.ExternalBreadcrumbs = new Breadcrumb[0];
        }

        public DateTime UpdatedAt { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationSubName { get; set; }

        public Breadcrumb[] ExternalBreadcrumbs { get; set; }

        public string[] Areas { get; set; }

        public string[] Releases { get; set; }

        public string MailFrom { get; set; }

        public string MailServer { get; set; }

        public int MailPort { get; set; }

        public bool MailSsl { get; set; }

        public string MailUsername { get; set; }

        public string MailPassword { get; set; }

        public Dictionary<string, object> PopulateWithData(NameValueCollection data)
        {
            Dictionary<string, object> updated = new Dictionary<string, object>();
            bool ssl = false;

            foreach (string name in data.AllKeys)
            {
                string[] values = data.GetValues(name);
                string unsafeValue = values[values.Length - 1];
                string value = unsafeValue; // Encoder.HtmlEncode(unsafeValue);
                switch (name.ToLowerInvariant())
                {
                    case "name":
                        this.ApplicationName = value;
                        updated.Add("ApplicationName", this.ApplicationName);
                        break;

                    case "subname":
                        this.ApplicationSubName = value;
                        updated.Add("ApplicationSubName", this.ApplicationSubName);
                        break;

                    case "areas":
                        this.Areas = value.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        updated.Add("Areas", this.Areas);
                        break;

                    case "releases":
                        this.Releases = value.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        updated.Add("Releases", this.Releases);
                        break;

                    case "breadcrumbs":
                        this.ExternalBreadcrumbs = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(s => { var a = s.Split(new[] { ',' }, 2, StringSplitOptions.None); return new Breadcrumb(a[0].Trim(), a[1].Trim()); })
                                                        .ToArray();
                        updated.Add("ExternalBreadcrumbs", this.ExternalBreadcrumbs);
                        break;

                    case "mail.from":
                        this.MailFrom = value;
                        updated.Add("MailFrom", this.MailFrom);
                        break;

                    case "mailserver":
                        this.MailServer = value;
                        updated.Add("MailServer", this.MailServer);
                        break;

                    case "mailport":
                        {
                            int port;
                            if (!Int32.TryParse(value, out port))
                            {
                                port = 25;
                            }
                            this.MailPort = port;
                            updated.Add("MailPort", this.MailPort);
                        }
                        break;

                    case "mailusername":
                        this.MailUsername = value;
                        updated.Add("MailUsername", this.MailUsername);
                        break;

                    case "mailpassword":
                        this.MailPassword = value;
                        updated.Add("MailPassword", this.MailPassword);
                        break;

                    case "mailssl":
                        ssl = true;
                        break;
                }
            }

            this.MailSsl = ssl;
            updated.Add("MailSsl", this.MailSsl);

            this.UpdatedAt = DateTime.UtcNow;
            updated.Add("UpdatedAt", this.UpdatedAt);

            return updated;
        }
    }
}
