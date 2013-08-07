namespace RobMensching.TinyBugs.ViewModels
{
    using System;

    public class RelativeDateViewModel
    {
        private DateTime datetime;

        public RelativeDateViewModel(DateTime datetime)
        {
            this.datetime = datetime;
        }

        public static implicit operator DateTime(RelativeDateViewModel vm)
        {
            return vm.datetime;
        }

        public string Date { get { return this.datetime.ToString("yyyy-MM-dd"); } }

        public string Friendly { get { return this.datetime == DateTime.MinValue ? "never" : this.datetime.ToString("D"); } }

        public string Relative
        {
            get
            {
                if (this.datetime == DateTime.MinValue)
                {
                    return "never";
                }

                // TODO: this doesn't actually handle "today" and "yesterday" correctly.
                TimeSpan diff = DateTime.UtcNow - this.datetime;
                if (diff.Days == 0)
                {
                    // time based.
                    return "earlier today";
                }
                else if (diff.Days == 1)
                {
                    return "yesterday";
                }
                else if (diff.Days < 25)
                {
                    return diff.Days + " days ago";
                }
                else if (diff.Days < 40)
                {
                    return "a month ago";
                }
                else if (diff.Days < 360)
                {
                    return diff.Days / 30 + " months ago";
                }
                else
                {
                    int halfyears = (int)(diff.Days / 182.5);
                    if (halfyears < 3)
                    {
                        return "last year";
                    }
                    else if (halfyears < 4)
                    {
                        return "a year and half ago";
                    }

                    return halfyears / 2 + " years ago";
                }
            }
        }

        public override string ToString()
        {
            return this.Relative;
        }
    }
}
