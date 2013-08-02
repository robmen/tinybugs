namespace RobMensching.TinyBugs.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Config
    {
        public string ApplicationName { get; set; }

        public string ApplicationSubName { get; set; }

        public string[] Areas { get; set; }

        public string[] Releases { get; set; }

        public DateTime UpdatedAt { get; set; }

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
                }
            }

            this.UpdatedAt = DateTime.UtcNow;
            updated.Add("UpdatedAt", this.UpdatedAt);

            return updated;
        }
    }
}
