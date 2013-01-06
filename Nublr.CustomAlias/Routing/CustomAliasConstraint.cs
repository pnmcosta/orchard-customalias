using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Nublr.CustomAlias.Routing
{
    [OrchardFeature("Nublr.CustomAlias")]
    public interface ICustomAliasConstraint : IRouteConstraint, ISingletonDependency
    {
        void SetAlias(IEnumerable<string> aliases);
        string FindAlias(string alias);
        void AddAlias(string alias);
        void RemoveAlias(string alias);
    }

    [OrchardFeature("Nublr.CustomAlias")]
    public class CustomAliasConstraint : ICustomAliasConstraint
    {
        /// <summary>
        /// Singleton object, per Orchard Shell instance. We need to protect concurrent access to the dictionary.
        /// </summary>
        private readonly object _syncLock = new object();
        private IDictionary<string, string> _slugs = new Dictionary<string, string>();

        public CustomAliasConstraint()
        {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void SetAlias(IEnumerable<string> slugs)
        {
            // Make a copy to avoid performing potential lazy computation inside the lock
            var slugsArray = slugs.ToArray();

            lock (_syncLock)
            {
                _slugs = slugsArray.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
            }

            Logger.Debug("Custom alias: {0}", string.Join(", ", slugsArray));
        }

        public string FindAlias(string slug)
        {
            lock (_syncLock)
            {
                string actual;
                return _slugs.TryGetValue(slug, out actual) ? actual : slug;
            }
        }

        public void AddAlias(string slug)
        {
            lock (_syncLock)
            {
                _slugs[slug] = slug;
            }
        }

        public void RemoveAlias(string slug)
        {
            lock (_syncLock)
            {
                _slugs.Remove(slug);
            }
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value))
            {
                var parameterValue = Convert.ToString(value);

                lock (_syncLock)
                {
                    return _slugs.ContainsKey(parameterValue);
                }
            }

            return false;
        }
    }
}