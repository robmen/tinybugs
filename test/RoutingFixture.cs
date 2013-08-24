namespace tinyBugs.test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Routing;
    using RobMensching.TinyWebStack;
    using Xunit;

    public class RoutingFixture
    {
        [Fact]
        public void RouteOrder()
        {
            RouteCollection collection = new RouteCollection();
            Routing.RegisterRoutes(collection);

            Route[] routes = collection.Cast<Route>().ToArray();
            Assert.Equal("a/b/c", routes[0].Url);
            Assert.Equal("a/b", routes[1].Url);
            Assert.Equal("a/{b}/{c}", routes[2].Url);
            Assert.Equal("a/{b}", routes[3].Url);
            Assert.Equal("a/{*b}", routes[4].Url);
            Assert.Equal("a", routes[5].Url);
            Assert.Equal(6, routes.Length);
        }
    }

    [Route("a/{*b}")]
    public class BAll : ControllerBase { }

    [Route("a/b")]
    [Route("a/b/c")]
    public class B : ControllerBase { }

    [Route("a/{b}")]
    [Route("a/{b}/{c}")]
    public class B2 : ControllerBase { }

    [Route("a")]
    public class A : ControllerBase { }
}
