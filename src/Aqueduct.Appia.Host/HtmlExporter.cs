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
        private readonly INancyBootstrapper _bootStrapper;
        private readonly string _exportPath;
        private readonly string _basePath;

        private List<string> _exludedFolders;
        public HtmlExporter(string exportPath,
            IConfiguration configuration,
            INancyBootstrapper bootStrapper)
        {
            _configuration = configuration;
            _exportPath = exportPath;
            _basePath = Directory.GetCurrentDirectory();
            _bootStrapper = bootStrapper;
        }

        public void Initialise()
        {
            _bootStrapper.Initialise();

            

            _exludedFolders = new List<string> { 
                            Path.Combine(_basePath, _configuration.LayoutsPath),
                            Path.Combine(_basePath, _configuration.HelpersPath),
                            Path.Combine(_basePath, _configuration.ModelsPath),
                            Path.Combine(_basePath, _configuration.PagesPath),
                            Path.Combine(_basePath, _configuration.PartialsPath)
                        };

            InitialiseExportPath();
        }

        public bool Verbose { get; set; }

        public void Export()
        {
            Log("Exporting {0} to {1}", _basePath, _exportPath);
            ExportDynamicPages();
            ExportStaticContent();

        }

        private void ExportDynamicPages()
        {
            IEnumerable<string> pages = GetAllPages();
            Log("{0} pages found", pages.Count());

            var engine = _bootStrapper.GetEngine();
            foreach (string page in pages)
            {
                Log("Processing page {0}", page);

                try
                {
                    var nancyRequest = ConvertPathToNancyRequest(page);
                    using (var nancyContext = engine.HandleRequest(nancyRequest))
                    {
                        ConvertNancyResponseToResponse(nancyContext.Response, GetPageExportPath(page));
                    }
                }
                catch (Exception ex)
                {
                    LogError("Processing page {0}", ex, page);
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

        private Request ConvertPathToNancyRequest(string pagePath)
        {
            //the result will be pages/test.cshtml
            string basePath = Path.Combine(_basePath, _configuration.PagesPath);
            string relativeUrl = pagePath.Replace(basePath, "").Replace(@"\", @"/").TrimStart('/');

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
                using (var reader = new StreamReader(output))
                {
                    File.WriteAllText(filePath, reader.ReadToEnd());
                }
            }

        }

        private void ExportStaticContent()
        {
            string basePath = _basePath.TrimEnd('\\') + "\\";

            var staticFiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);

            foreach (string staticFile in staticFiles)
            {

                if (ShouldCopy(staticFile))
                {
                    Log("Copying file {0}", staticFile);
                    CopyToExportFolder(staticFile);
                }
                else
                    Log("Skipping file {0}", staticFile);
                
            }
        }

        private bool ShouldCopy(string filePath)
        {
            foreach (var excludedFolder in _exludedFolders)
            {
                if (filePath.StartsWith(excludedFolder, StringComparison.CurrentCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        private void CopyToExportFolder(string staticFile)
        {
            var destination = staticFile.Replace(_basePath, _exportPath);
            try
            {
                Directory.CreateDirectory(destination.Substring(0, destination.LastIndexOf('\\')));
                File.Copy(staticFile, destination);
            }
            catch (Exception ex)
            {
                LogError("Processing copying file {0} to {1}", ex, staticFile, destination);
            }
        }
        private void Log(string message, params object[] args)
        {
            if (Verbose)
                Console.WriteLine(message, args);
        }

        private void LogError(string message, Exception ex, params object[] args)
        {
            Console.WriteLine("Error: " + message, args);
            if (Verbose)
                Console.WriteLine("Message: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
        }
    }
}
