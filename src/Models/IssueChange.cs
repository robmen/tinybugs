namespace RobMensching.TinyBugs.Models
{
    using System;

    public class IssueChange : IComparable
    {
        public string Column { get; set; }

        public string Old { get; set; }

        public string New { get; set; }

        public int CompareTo(object obj)
        {
            IssueChange other = obj as IssueChange;
            if (other == null)
            {
                return -1;
            }
            else
            {
                return this.Column.CompareTo(other.Column);
            }
        }
    }
}
