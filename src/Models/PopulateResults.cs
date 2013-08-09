namespace RobMensching.TinyBugs.Models
{
    using System.Collections.Generic;

    public class PopulateResults
    {
        public PopulateResults()
        {
            this.Updates = new Dictionary<string, object>();
            this.Errors = new List<ValidationError>();
        }

        public Dictionary<string, object> Updates { get; private set; }

        public List<ValidationError> Errors { get; private set; }
    }
}
