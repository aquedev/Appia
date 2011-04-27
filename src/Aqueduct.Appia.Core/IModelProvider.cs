using System;

namespace Aqueduct.Appia.Core
{
    public interface IModelProvider
    {
        dynamic GetGlobalModel();
        dynamic GetModel(string viewName);
    }

    public interface IHelpersProvider
    {
        string GetGlobalHelpersContent();
        string GetHelpersContent(string helperName);
    }
}
