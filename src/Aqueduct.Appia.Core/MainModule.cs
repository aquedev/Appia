namespace Aqueduct.Appia.Core
{
    using System;
    using Nancy.Routing;
    using Nancy;


    public class MainModule : NancyModule
    {
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

            Get["/(?<path>images|img)/{file}"] = x =>
            {
                return Response.AsImage(String.Format("{0}/{1}", (string)x.path, (string)x.file));
            };

            Get["/"] = x => {
                return View["index"];
            };

            Get["/{path}", (ctx) => ctx.Request.Uri != "/"] = x =>
            {
                return View[x.path];
            };

            
        }
    }
}