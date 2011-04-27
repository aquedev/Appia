using Aqueduct.Appia.Core;
using Nancy;
using System.IO;
using System;

namespace Aqueduct.FrontEnd.Site
{
    public class HelpersProvider : IHelpersProvider
    {
        private readonly IRootPathProvider _rootPathProvider;
        private readonly IConfiguration _settings;

        public HelpersProvider(IRootPathProvider rootPathProvider, IConfiguration settings)
        {
            _settings = settings;
            _rootPathProvider = rootPathProvider;
        }

        public string GetGlobalHelpersContent()
        {
            string globalHelpersPath = Path.Combine(_rootPathProvider.GetRootPath(), _settings.HelpersPath, Conventions.GlobalHelpersFile);

            if (File.Exists(globalHelpersPath) == false)
                return String.Empty;

            using (var bodyStream = new StreamReader(globalHelpersPath))
            {
                return bodyStream.ReadToEnd();
            }
        }

        public string GetHelpersContent(string helperName)
        {
            throw new NotImplementedException();
        }

    }
}
