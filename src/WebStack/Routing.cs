namespace RobMensching.TinyWebStack
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Web.Routing;

    public static class Routing
    {
        public static RouteCollection RegisterRoutes(RouteCollection collection = null)
        {
            collection = collection ?? RouteTable.Routes;

            var routedTypes = from t in Assembly.GetCallingAssembly().GetTypes().AsParallel()
                              let attributes = t.GetCustomAttributes(typeof(RouteAttribute), true)
                              where attributes != null && attributes.Length > 0
                              select new { Type = t, Attributes = attributes.Cast<RouteAttribute>() };
            foreach (var routedType in routedTypes)
            {
                ControllerBase controller = (ControllerBase)Activator.CreateInstance(routedType.Type);
                foreach (var routeAttribute in routedType.Attributes)
                {
                    collection.Add(new Route(routeAttribute.Path, controller.RouteDefaults, controller.RouteConstraints, controller));
                }
            }

            return collection;
        }
    }
}
