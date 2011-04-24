using System.Configuration;
using Aqueduct.Appia.Core;

namespace Aqueduct.Appia.Core
{
    public class Configuration : IConfiguration
    {
        public string PagesPath
        {
            get { return GetSetting("TemplatesPath", "pages"); }
        }

        public string LayoutsPath
        {
            get { return GetSetting("LayoutsPath", "layouts"); }
        }

        public string ModelsPath
        {
            get { return GetSetting("ModelsPath", "models"); }
        }

        public string PartialsPath
        {
            get { return GetSetting("PartialsPath", "partials"); }
        }

        private static string GetSetting(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
        
    }
}
