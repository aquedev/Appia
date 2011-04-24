namespace Aqueduct.Appia.Razor
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Razor;
    using Microsoft.CSharp;
    using Aqueduct.Appia.Core;
    using Nancy.ViewEngines;

    public class RazorViewEngine : IViewEngine
    {
        private readonly RazorTemplateEngine engine;
        private readonly CodeDomProvider _codeDomProvider;
        private readonly IViewLocator _locator;
        private readonly IConfiguration _settings;
        private readonly IModelProvider _modelProvider;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RazorViewEngine"/> class.
        /// </summary>
        public RazorViewEngine(IViewLocator locator, IConfiguration settings, IModelProvider modelProvider)
            : this(GetRazorTemplateEngine(), new CSharpCodeProvider(), locator, settings, modelProvider)
        {
        }

        public RazorViewEngine(RazorTemplateEngine razorTemplateEngine, 
            CodeDomProvider codeDomProvider, 
            IViewLocator locator,
            IConfiguration settings, IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
            _settings = settings;
            engine = razorTemplateEngine;
            _codeDomProvider = codeDomProvider;
            _locator = locator;
        }

        private static RazorTemplateEngine GetRazorTemplateEngine()
        {
            var host =
                new RazorEngineHost(new CSharpRazorCodeLanguage())
                {
                    DefaultBaseClass = typeof(ViewBase).FullName,
                    DefaultNamespace = "RazorOutput",
                    DefaultClassName = "RazorView"
                };

            host.NamespaceImports.Add("System");
            host.NamespaceImports.Add("System.IO");
            host.NamespaceImports.Add("System.Collections.Generic");
            host.NamespaceImports.Add("Microsoft.CSharp.RuntimeBinder");
            host.NamespaceImports.Add("Aqueduct.Appia.Core");
            
            return new RazorTemplateEngine(host);
        }

        private ViewBase GetCompiledView<TModel>(TextReader reader)
        {
            var razorResult = engine.GenerateCode(reader);

            return GenerateRazorView(_codeDomProvider, razorResult);
        }

        private static ViewBase GenerateRazorView(CodeDomProvider codeProvider, GeneratorResults razorResult)
        {
            // Compile the generated code into an assembly

            var outputAssemblyName =
                Path.Combine(Path.GetTempPath(), String.Format("Temp_{0}.dll", Guid.NewGuid().ToString("N")));

            var results = codeProvider.CompileAssemblyFromDom(
                new CompilerParameters(new[] {
                    GetAssemblyPath(typeof(Microsoft.CSharp.RuntimeBinder.Binder)), 
                    GetAssemblyPath(typeof(System.Runtime.CompilerServices.CallSite)), 
                    GetAssemblyPath(typeof(ViewBase)),
                    GetAssemblyPath(Assembly.GetExecutingAssembly())}, outputAssemblyName),
                    razorResult.GeneratedCode);

            if (results.Errors.HasErrors)
            {
                var err = results.Errors
                    .OfType<CompilerError>()
                    .Where(ce => !ce.IsWarning)
                    .First();

                var error = String.Format("Error Compiling Template: ({0}, {1}) {2})", err.Line, err.Column, err.ErrorText);

                return new NancyRazorErrorView(error);
            }
            // Load the assembly
            var assembly = Assembly.LoadFrom(outputAssemblyName);
            if (assembly == null)
            {
                const string error = "Error loading template assembly";
                return new NancyRazorErrorView(error);
            }

            // Get the template type
            var type = assembly.GetType("RazorOutput.RazorView");
            if (type == null)
            {
                var error = String.Format("Could not find type RazorOutput.Template in assembly {0}", assembly.FullName);
                return new NancyRazorErrorView(error);
            }

            var view = Activator.CreateInstance(type) as ViewBase;
            if (view == null)
            {
                const string error = "Could not construct RazorOutput.Template or it does not inherit from RazorViewBase";
                return new NancyRazorErrorView(error);
            }

            return view;
        }

        private static string GetAssemblyPath(Type type)
        {
            return GetAssemblyPath(type.Assembly);
        }

        private static string GetAssemblyPath(Assembly assembly)
        {
            return new Uri(assembly.CodeBase).LocalPath;
        }

        /// <summary>
        /// Gets the extensions file extensions that are supported by the view engine.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> instance containing the extensions.</value>
        /// <remarks>The extensions should not have a leading dot in the name.</remarks>
        public IEnumerable<string> Extensions
        {
            get { return new[] { "cshtml", "vbhtml" }; }
        }

        /// <summary>
        /// Renders the view.
        /// </summary>
        /// <param name="viewLocationResult">A <see cref="ViewLocationResult"/> instance, containing information on how to get the view template.</param>
        /// <param name="model">The model that should be passed into the view</param>
        /// <returns>A delegate that can be invoked with the <see cref="Stream"/> that the view should be rendered to.</returns>
        public Action<Stream> RenderView(ViewLocationResult viewLocationResult, dynamic model)
        {
            return stream =>
            {
                var writer =
                    new StreamWriter(stream);

                writer.Write(ExecuteView(viewLocationResult, model));

                writer.Flush();
            };
        }

        private string ExecuteView(string path, dynamic model)
        {
            var layoutContent = _locator.GetViewLocation(path, Extensions);
            if (layoutContent != null)
                return ExecuteView(layoutContent, model);
            else
                return String.Empty;
        }

        private string ExecuteView(ViewLocationResult viewLocationResult, dynamic model)
        {
            var view =
                    GetCompiledView<dynamic>(viewLocationResult.Contents);
            view.Global = _modelProvider.GetGlobalModel();
            view.Model = model ?? _modelProvider.GetModel(Path.GetFileNameWithoutExtension(viewLocationResult.Location));

            view.RenderPartialImpl = (partialViewName, partialModel)
                => new HttpStringLiteral(ExecuteView(Conventions.PartialsPrefix + partialViewName, partialModel));
            view.Execute();

            if (string.IsNullOrEmpty(view.Layout))
            {
                return view.Contents;
            }
            else
            {
                string layout = ExecuteView(String.Format("{0}{1}", Conventions.LayoutsPrefix, view.Layout), model);

                return layout.Replace("{{content}}", view.Contents);
            }
        }
    }
}