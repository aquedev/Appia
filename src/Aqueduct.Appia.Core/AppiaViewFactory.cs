namespace Aqueduct.Appia.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Nancy.ViewEngines;
    using Nancy;

    /// <summary>
    /// The default implementation for how views are resolved and rendered by Nancy.
    /// </summary>
    public class AppiaViewFactory : IViewFactory
    {
        private readonly IViewLocator viewLocator;
        private readonly IEnumerable<IViewEngine> viewEngines;
        private static readonly Action<Stream> NullView = null;

        public AppiaViewFactory(IViewLocator viewLocator, IEnumerable<IViewEngine> viewEngines)
        {
            this.viewLocator = viewLocator;
            this.viewEngines = viewEngines;
        }

        
        public Action<Stream> RenderView(NancyModule module, string viewName, dynamic model)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module", "The value of the module parameter cannot be null.");
            }

            if (viewName == null && model == null)
            {
                throw new ArgumentException("viewName and model parameters cannot both be null.");
            }

            if (model == null && viewName.Length == 0)
            {
                throw new ArgumentException("The viewName parameter cannot be empty when the model parameters is null.");
            }

            var actualViewName =
                viewName ?? GetViewNameFromModel(model);

            return GetRenderedView(actualViewName, model);
        }

        public Action<Stream> this[dynamic model]
        {
            get { return GetRenderedView(GetViewNameFromModel(model), model); }
        }

        public Action<Stream> this[string viewName]
        {
            get { return GetRenderedView(viewName, null); }
        }

        public Action<Stream> this[string viewName, dynamic model]
        {
            get { return GetRenderedView(viewName, model); }
        }

        private IEnumerable<string> GetExtensionsToUseForViewLookup(string viewName)
        {
            var extensions =
                GetViewExtension(viewName) ?? GetSupportedViewEngineExtensions();

            return extensions;
        }

        private Action<Stream> GetRenderedView(string viewName, dynamic model)
        {
            var viewLocationResult =
                viewLocator.GetViewLocation(Path.GetFileNameWithoutExtension(viewName), this.GetExtensionsToUseForViewLookup(viewName));

            var resolvedViewEngine =
                GetViewEngine(viewLocationResult);

            if (resolvedViewEngine == null)
            {
                return NullView;
            }
            
            return SafeInvokeViewEngine(
                resolvedViewEngine,
                viewLocationResult,
                model
            );
        }

        private IEnumerable<string> GetSupportedViewEngineExtensions()
        {
            var viewEngineExtensions =
                viewEngines.SelectMany(x => x.Extensions);

            return viewEngineExtensions.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IViewEngine GetViewEngine(ViewLocationResult viewLocationResult)
        {
            if (viewLocationResult == null)
            {
                return null;
            }

            var viewEngiens =
                from viewEngine in viewEngines
                where viewEngine.Extensions.Any(x => x.Equals(viewLocationResult.Extension, StringComparison.InvariantCultureIgnoreCase))
                select viewEngine;

            return viewEngiens.FirstOrDefault();
        }

        private static IEnumerable<string> GetViewExtension(string viewName)
        {
            var extension =
                Path.GetExtension(viewName);

            return string.IsNullOrEmpty(extension) ? null : new[] { extension.TrimStart('.') };
        }

        private static string GetViewNameFromModel(dynamic model)
        {
            return Regex.Replace(model.GetType().Name, "Model$", string.Empty);
        }

        private static Action<Stream> SafeInvokeViewEngine(IViewEngine viewEngine, ViewLocationResult locationResult, dynamic model)
        {
            try
            {
                return viewEngine.RenderView(locationResult, model);
            }
            catch (Exception)
            {
                return NullView;
            }
        }
    }
}