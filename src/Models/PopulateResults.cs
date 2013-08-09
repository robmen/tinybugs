namespace RobMensching.TinyBugs.Models
{
    using System.Collections.Generic;

    public class PopulateResults
    {
        public PopulateResults()
        {
            this.Updates = new Dictionary<string, UpdatedValue>();
            this.Errors = new List<ValidationError>();
        }

        public Dictionary<string, UpdatedValue> Updates { get; private set; }

        public List<ValidationError> Errors { get; private set; }

        public class UpdatedValue
        {
            public string FriendlyName { get; set; }

            public string FriendlyOld { get; set; }

            public string FriendlyNew { get; set; }

            public object Old { get; set; }

            public object New { get; set; }
        }
    }
}
