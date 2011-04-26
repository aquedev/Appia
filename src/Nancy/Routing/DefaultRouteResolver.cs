﻿namespace Nancy.Routing
{
    using System.Collections.Generic;
    using System.Linq;

    using RouteCandidate = System.Tuple<string, int, RouteDescription, IRoutePatternMatchResult>;
    using ResolveResult = System.Tuple<Route, DynamicDictionary, System.Func<NancyContext, Response>, System.Action<NancyContext>>;

    /// <summary>
    /// The default implementation for deciding if any of the available routes is a match for the incoming HTTP request.
    /// </summary>
    public class DefaultRouteResolver : IRouteResolver
    {
        private readonly INancyModuleCatalog nancyModuleCatalog;
        private readonly IRoutePatternMatcher routePatternMatcher;
        private readonly INancyModuleBuilder moduleBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRouteResolver"/> class.
        /// </summary>
        /// <param name="nancyModuleCatalog">The module catalog that modules should be</param>
        /// <param name="routePatternMatcher">The route pattern matcher that should be used to verify if the route is a match to any of the registered routes.</param>
        /// <param name="moduleBuilder">The module builder that will make sure that the resolved module is full configured.</param>
        public DefaultRouteResolver(INancyModuleCatalog nancyModuleCatalog, IRoutePatternMatcher routePatternMatcher, INancyModuleBuilder moduleBuilder)
        {
            this.nancyModuleCatalog = nancyModuleCatalog;
            this.routePatternMatcher = routePatternMatcher;
            this.moduleBuilder = moduleBuilder;
        }

        /// <summary>
        /// Gets the route, and the corresponding parameter dictionary from the URL
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="routeCache">Route cache</param>
        /// <returns>Tuple - Item1 being the Route, Item2 being the parameters dictionary, Item3 being the prereq, Item4 being the postreq</returns>
        public ResolveResult Resolve(NancyContext context, IRouteCache routeCache)
        {
            if (routeCache.IsEmpty())
            {
                return new ResolveResult(new NotFoundRoute(context.Request.Method, context.Request.Uri), DynamicDictionary.Empty, null, null);
            }

            var routesThatMatchRequestedPath = this.GetRoutesThatMatchRequestedPath(routeCache, context);

            if (NoRoutesWereAbleToBeMatchedInRouteCache(routesThatMatchRequestedPath))
            {
                return new ResolveResult(new NotFoundRoute(context.Request.Method, context.Request.Uri), DynamicDictionary.Empty, null, null);
            }

            var routesWithCorrectRequestMethod =
                GetRoutesWithCorrectRequestMethod(context.Request, routesThatMatchRequestedPath);

            if (NoRoutesWereForTheRequestedMethod(routesWithCorrectRequestMethod))
            {
                return new ResolveResult(new MethodNotAllowedRoute(context.Request.Uri, context.Request.Method), DynamicDictionary.Empty, null, null);
            }

            var routeMatchesWithMostParameterCaptures = 
                GetRouteMatchesWithMostParameterCaptures(routesWithCorrectRequestMethod);

            var routeMatchToReturn = 
                GetSingleRouteToReturn(routeMatchesWithMostParameterCaptures);

            return this.CreateRouteAndParametersFromMatch(context, routeMatchToReturn);
        }

        private ResolveResult CreateRouteAndParametersFromMatch(NancyContext context, RouteCandidate routeMatchToReturn)
        {
            var associatedModule =
                this.GetInitializedModuleForMatch(context, routeMatchToReturn);

            var route = associatedModule.Routes.ElementAt(routeMatchToReturn.Item2);

            return new ResolveResult(route, routeMatchToReturn.Item4.Parameters, associatedModule.Before, associatedModule.After);
        }

        private NancyModule GetInitializedModuleForMatch(NancyContext context, RouteCandidate routeMatchToReturn)
        {
            var module =
                this.nancyModuleCatalog.GetModuleByKey(routeMatchToReturn.Item1, context);

            return this.moduleBuilder.BuildModule(module, context);
        }

        private static RouteCandidate GetSingleRouteToReturn(IEnumerable<RouteCandidate> routesWithMostParameterCaptures)
        {
            return routesWithMostParameterCaptures.First();
        }

        private static IEnumerable<RouteCandidate> GetRouteMatchesWithMostParameterCaptures(IEnumerable<RouteCandidate> routesWithCorrectRequestMethod)
        {
            var maxParameterCount =
                routesWithCorrectRequestMethod.Max(x => x.Item4.Parameters.GetDynamicMemberNames().Count());

            return routesWithCorrectRequestMethod.Where(
                x => x.Item4.Parameters.GetDynamicMemberNames().Count() == maxParameterCount);
        }

        private static bool NoRoutesWereForTheRequestedMethod(IEnumerable<RouteCandidate> routesWithCorrectRequestMethod)
        {
            return !routesWithCorrectRequestMethod.Any();
        }

        private static IEnumerable<RouteCandidate> GetRoutesWithCorrectRequestMethod(Request request, IEnumerable<RouteCandidate> routesThatMatchRequestedPath)
        {
            return from route in routesThatMatchRequestedPath
                   let routeMethod = route.Item3.Method.ToUpperInvariant()
                   let requestMethod = request.Method.ToUpperInvariant()
                   where routeMethod.Equals(requestMethod) || (routeMethod.Equals("GET") && requestMethod.Equals("HEAD"))                    
                   select route;
        }

        private static bool NoRoutesWereAbleToBeMatchedInRouteCache(IEnumerable<RouteCandidate> routesThatMatchRequestedPath)
        {
            return !routesThatMatchRequestedPath.Any();
        }

        private IEnumerable<RouteCandidate> GetRoutesThatMatchRequestedPath(IRouteCache routeCache, NancyContext context)
        {
            return from cacheEntry in routeCache
                   from cacheEntryRoutes in cacheEntry.Value
                   let routeIndex = cacheEntryRoutes.Item1
                   let routeDescription = cacheEntryRoutes.Item2
                   where ((routeDescription.Condition == null) || (routeDescription.Condition(context)))
                   let result = this.routePatternMatcher.Match(context.Request.Uri, routeDescription.Path)
                   where result.IsMatch
                   select new RouteCandidate(cacheEntry.Key, routeIndex, routeDescription, result);
        }
    }
}
