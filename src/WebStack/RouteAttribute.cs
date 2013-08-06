namespace RobMensching.TinyWebStack
{
    using System;

    [Flags]
    public enum RouteVerbs
    {
        None = 0x0,
        Get = 0x1,
        Post = 0x2,
        Put = 0x4,
        Delete = 0x8,
        //Patch = 0x10,
        //Head = 0x20,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(string path, RouteVerbs verbs = RouteVerbs.Get | RouteVerbs.Post | RouteVerbs.Put | RouteVerbs.Delete)
        {
            this.Path = path;
            this.Verbs = verbs;
        }

        public string Path { get; set; }

        public RouteVerbs Verbs { get; set; }
    }
}
