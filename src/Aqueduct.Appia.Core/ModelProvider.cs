using Aqueduct.Appia.Core;
using Nancy;
using System.IO;
using System;

namespace Aqueduct.FrontEnd.Site
{
    public class ModelProvider : IModelProvider
    {
        private readonly IRootPathProvider _rootPathProvider;
        private readonly IConfiguration _settings;
        /// <summary>
        /// Initializes a new instance of the GlobalModelManager class.
        /// </summary>
        public ModelProvider(IRootPathProvider rootPathProvider, IConfiguration settings)
        {
            _settings = settings;
            _rootPathProvider = rootPathProvider;
        }

        public dynamic GetGlobalModel()
        {
        
            string globalModelPath = Path.Combine(_rootPathProvider.GetRootPath(), _settings.ModelsPath, Conventions.GlobalModelFile);

            if (File.Exists(globalModelPath) == false)
                return null;

            var bodyStream = new StreamReader(globalModelPath);
            try
            {
                var json = bodyStream.ReadToEnd();
                dynamic globalModel = JsonHelpers.ParseJsonObject(json);
                return globalModel;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while parsing global model", ex);
            }
            finally
            {
                if (bodyStream != null)
                    bodyStream.Dispose();
            }
        }

        public dynamic GetModel(string viewName)
        {
            string modelPath = Path.Combine(_rootPathProvider.GetRootPath(), _settings.ModelsPath, viewName + ".js");

            if (File.Exists(modelPath) == false)
                return null;

            var bodyStream = new StreamReader(modelPath);
            try
            {
                var json = bodyStream.ReadToEnd();
                return JsonHelpers.ParseJsonObject(json);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while parsing global model", ex);
            }
            finally
            {
                if (bodyStream != null)
                    bodyStream.Dispose();
            }
        }
    }
}
