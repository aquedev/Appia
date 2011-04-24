using System;

namespace Aqueduct.Appia.Core
{
    public interface IModelProvider
    {
        dynamic GetGlobalModel();
        dynamic GetModel(string viewName);
    }
}
