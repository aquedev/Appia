namespace Aqueduct.Appia.Core
{
    using System;
    using Nancy.Routing;
    using Nancy;
    using System.Linq;


    public class MainModule : NancyModule
    {
        public readonly string[] ForbiddenExtensions = { "config", "pdb", "dll" };

        public MainModule(IRouteCacheProvider routeCacheProvider)
        {
            Get["/css/{file}"] = x => {
                return Response.AsCss("css/" + (string)x.file);
            };

            Get["/js/{file}"] = x =>
            {
                return Response.AsJs("js/" + (string)x.file);
            };

            Get["/lib/{file}"] = x =>
            {
                return Response.AsFile("lib/" + (string)x.file);
            };

            Get["/"] = x => {
                return View["index"];
            };

            Get["/{path}", (ctx) => ctx.Request.Uri != "/"] = x =>
            {
                return ViewOrFallback((string)x.path, () => 
                {
                    dynamic result = IsAllowedFile((string)x.path) ? Response.AsFile((string)x.path) : 403;

                    return result;
                });
            };
        }

        private bool IsAllowedFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            string processed = path.Trim('/').ToLower();

            if (processed.StartsWith("bin"))
                return false;

            var extension = System.IO.Path.GetExtension(processed).Trim('.');
            return ForbiddenExtensions.Contains(extension) == false;
        }

        private dynamic ViewOrFallback(string viewPath, Func<dynamic> fallback)
        {
            return View[viewPath] ?? fallback.Invoke();
        }
    }
}