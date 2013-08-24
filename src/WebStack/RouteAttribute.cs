namespace RobMensching.TinyWebStack
{
    using System;
    using System.Linq;
    using System.Web.Routing;

    [Flags]
    public enum RouteVerbs
    {
        None = 0x0,
        Get = 0x1,
        Post = 0x2,
        Put = 0x4,
        Delete = 0x8,
        Patch = 0x10,
        Head = 0x20,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RouteAttribute : Attribute, IComparable<RouteAttribute>
    {
        public RouteAttribute(string path)
        {
            this.Path = path;
            this.Defaults = null;
            this.Verbs = RouteVerbs.Get | RouteVerbs.Post | RouteVerbs.Put | RouteVerbs.Delete;
        }

        public string Path { get; set; }

        public RouteValueDictionary Defaults { get; set; }

        public RouteVerbs Verbs { get; set; }

        public int Complexity { get { return this.Path.Split('/').Length; } }

        public int CompareTo(RouteAttribute other)
        {
            int result = 0;
            var thisSegments = this.Path.Split('/').Select(s => new { Path = s, Variable = s.StartsWith("{"), Wildcard = s.StartsWith("{*") }).ToArray();
            var otherSegments = other.Path.Split('/').Select(s => new { Path = s, Variable = s.StartsWith("{"), Wildcard = s.StartsWith("{*") }).ToArray();

            for (int i = 0; result == 0 && i < thisSegments.Length && i < otherSegments.Length; ++i)
            {
                var thisSegment = thisSegments[i];
                var otherSegment = otherSegments[i];

                // A route with a wildcard goes last.
                if (thisSegment.Wildcard ^ otherSegment.Wildcard)
                {
                    result = thisSegment.Wildcard ? 1 : -1;
                }
                else if (thisSegment.Variable ^ otherSegment.Variable) // a route with variables goes later.
                {
                    result = thisSegment.Variable ? 1 : -1;
                }
                else // otherwise routes are ordered alphabetically.
                {
                    result = thisSegment.Path.CompareTo(otherSegment.Path);
                }
            }

            // If all segments (that exist) are equal, the more specific route (the one with
            // the longest path) goes first.
            if (result == 0)
            {
                result = thisSegments.Length.CompareTo(otherSegments.Length) * -1;
            }

            return result;
        }
    }
}
