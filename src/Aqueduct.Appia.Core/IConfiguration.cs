using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aqueduct.Appia.Core
{
    public interface IConfiguration
    {
        string PagesPath { get; }
        string LayoutsPath { get; }
        string ModelsPath { get; }
        string PartialsPath { get; }
        string HelpersPath { get; }
    }
}