using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Nublr.CustomAlias.Routing;
using Orchard;
using Orchard.Environment.Extensions;

namespace Nublr.CustomAlias
{
    [OrchardFeature("Nublr.CustomAlias")]
    public class Routes : IRouteProvider {
        private readonly ICustomAliasConstraint _customAliasSlugConstraint;
        public Routes(ICustomAliasConstraint customAliasSlugConstraint)
        {
            _customAliasSlugConstraint = customAliasSlugConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                    Priority = 85 /*higher than alias*/,
                                                    Route = new Route(
                                                         "{*customAlias}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Nublr.CustomAlias"},
                                                                                      {"controller", "Home"},
                                                                                      {"action", "Go"},
                                                                                      {"customAlias", ""}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"customAlias", _customAliasSlugConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Nublr.CustomAlias"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}