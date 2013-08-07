namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.Specialized;

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
                }
            }

            this.UpdatedAt = DateTime.UtcNow;
            updated.Add("UpdatedAt", this.UpdatedAt);

            return updated;
        }
    }
}
