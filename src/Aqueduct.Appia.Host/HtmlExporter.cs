using System;
using System.Collections.Generic;
using Nancy.Bootstrapper;
using Nancy;
using System.IO;
using Aqueduct.Appia.Core;
using System.Linq;

namespace Aqueduct.Appia.Host
{
    public class HtmlExporter
    {
        private readonly IConfiguration _configuration;
        private readonly INancyEngine engine;
        private readonly string _exportPath;
        private readonly string _basePath;

        public HtmlExporter(string exportPath, 
            IConfiguration configuration, 
            INancyBootstrapper bootStrapper)
        {
            _configuration = configuration;
            _exportPath = exportPath;
            bootStrapper.Initialise();
            
            engine = bootStrapper.GetEngine();
            _basePath = Directory.GetCurrentDirectory();
        }

        public void Export()
        {
            Log("Exporting {0} to {1}", _basePath, _exportPath);
            InitialiseExportPath();
            IEnumerable<string> pages = GetAllPages();
            Log("{0} pages found", pages.Count());
            foreach (string page in pages)
            {
                Log("Processing page {0}", page);

                var nancyRequest = ConvertPathToNancyRequest(page);
                using (var nancyContext = engine.HandleRequest(nancyRequest))
                {
                    ConvertNancyResponseToResponse(nancyContext.Response, GetPageExportPath(page));
                }
            }
        }

        private void InitialiseExportPath()
        {
            Log("InitialisingExportPath");
            string exportPath = Path.Combine(_basePath, _exportPath);
            if (Directory.Exists(exportPath))
                Directory.Delete(exportPath, true);

            Directory.CreateDirectory(exportPath);
        }

        private string GetPageExportPath(string pagePath)
        {
            return Path.Combine(_exportPath, Path.GetFileNameWithoutExtension(pagePath)) + ".html";
        }
        private IEnumerable<string> GetAllPages()
        {

            string pagesPath = Path.Combine(_basePath, _configuration.PagesPath);
            if (Directory.Exists(pagesPath) == false)
                throw new DirectoryNotFoundException(String.Format("Cannot find the pages folder. Make sure '{0}' exists under the current directory", _configuration.PagesPath));
            string[] pages = Directory.GetFiles(pagesPath);
            return pages;
        }

        private static Uri GetUrlAndPathComponents(Uri uri)
        {
            // ensures that for a given url only the
            //  scheme://host:port/paths/somepath
            return new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
        }

        private Request ConvertPathToNancyRequest(string pagePath)
        {
            string relativeUrl = pagePath.Replace(_basePath, "").Replace(@"\", @"/").TrimStart('/');
            if (relativeUrl.LastIndexOf('.') > 0)
                relativeUrl = relativeUrl.Substring(0, relativeUrl.LastIndexOf('.'));
            return new Request(
                "GET",
                string.Concat("/", relativeUrl),
                "http");
        }

        private static void ConvertNancyResponseToResponse(Response nancyResponse, string filePath)
        {
            using (var output = new MemoryStream())
            {
                nancyResponse.Contents.Invoke(output);
                output.Flush();
                output.Position = 0;
                using(var reader = new StreamReader(output))
                {
                    File.WriteAllText(filePath, reader.ReadToEnd());
                }
            }
            
        }
        private static void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}
