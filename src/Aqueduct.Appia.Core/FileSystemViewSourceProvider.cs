namespace Aqueduct.Appia.Core
{
    using Nancy.Session;
    using Nancy;
    using Nancy.ViewEngines;
    using System.Collections.Generic;
    using System.IO;
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class FileSystemViewSourceProvider : IViewSourceProvider
    {
        private readonly IRootPathProvider rootPathProvider;
        private readonly IConfiguration _settings;

        public FileSystemViewSourceProvider(IRootPathProvider rootPathProvider, IConfiguration settings)
        {
            _settings = settings;
            this.rootPathProvider = rootPathProvider;
        }

        /// <summary>
        /// Attemptes to locate the view, specified by the <paramref name="viewName"/> parameter, in the underlaying source.
        /// </summary>
        /// <param name="viewName">The name of the view that should be located.</param>
        /// <param name="supportedViewEngineExtensions">The supported view engine extensions that the view is allowed to use.</param>
        /// <returns>A <see cref="ViewLocationResult"/> instance if the view could be located; otherwise <see langword="null"/>.</returns>
        public ViewLocationResult LocateView(string viewName, IEnumerable<string> supportedViewEngineExtensions)
        {
            string prefix = GetViewPrefix(viewName);
            string processedViewName = GetViewName(viewName);

            var viewFolder = GetViewFolder(prefix, viewName);

            if (string.IsNullOrEmpty(viewFolder))
            {
                return null;
            }

            var filesInViewFolder =
                Directory.GetFiles(viewFolder);

            var viewsFiles =
                from file in filesInViewFolder
                from extension in supportedViewEngineExtensions
                where Path.GetFileName(file).Equals(string.Concat(processedViewName, ".", extension), StringComparison.OrdinalIgnoreCase)
                select new
                {
                    file,
                    extension
                };

            var selectedView =
                viewsFiles.FirstOrDefault();

            var fileStream = new FileStream(selectedView.file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return new ViewLocationResult(
                selectedView.file,
                selectedView.extension,
                new StreamReader(fileStream)
            );
        }

        private static string GetViewName(string viewName)
        {
            string viewWithOutPrefix = RemoveViewPrefix(viewName);
            if (viewWithOutPrefix.StartsWith("/"))
                return viewWithOutPrefix.Substring(viewWithOutPrefix.LastIndexOf("/") + 1);
            else
                return viewWithOutPrefix;
        }

        private static string RemoveViewPrefix(string viewName)
        {
            Match match = Regex.Match(viewName, Conventions.ViewPrefixPattern);
            if (match.Success)
                return viewName.Substring(match.Value.Length);
            return viewName;
        }
        private string GetViewPrefix(string viewName)
        {
            Match match = Regex.Match(viewName, Conventions.ViewPrefixPattern);

            if (match.Success)
                return match.Value;

            return String.Empty;
        }

        private string GetViewFolder(string prefix, string viewName)
        {
            string viewFolder = "";
            switch(prefix.ToLower())
            {
                case Conventions.LayoutsPrefix:
                    viewFolder = _settings.LayoutsPath;
                    break;
                case Conventions.PartialsPrefix:
                    viewFolder = _settings.PartialsPath;
                    break;
                default:
                    viewFolder = _settings.PagesPath;
                    break;
            }
            string viewSubfolder = String.Empty;
            string viewNameWithoutPrefix = RemoveViewPrefix(viewName);
            if (viewNameWithoutPrefix.StartsWith("/"))
                viewSubfolder = viewNameWithoutPrefix.Substring(1 /* no need for the beginning / */,
                                                                viewNameWithoutPrefix.LastIndexOf("/"));

            return Path.Combine(this.rootPathProvider.GetRootPath(), viewFolder, viewSubfolder.Replace('/', '\\'));
        }
    }
}
