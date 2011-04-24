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

            Get["/"] = x => {
                return View["index"];
            };

            Get["/{path}", (ctx) => ctx.Request.Uri != "/"] = x =>
            {
                return View[x.path];
            };

            //Get[@"/(?<foo>\d{2,4})/{bar}"] = x =>
            //{
            //    return string.Format("foo: {0}<br/>bar: {1}", x.foo, x.bar);
            //};

            //Get["/json"] = x => {
            //    var model = new RatPack { FirstName = "Andy" };
            //    return Response.AsJson(model);
            //};

            //Get["/xml"] = x => {
            //    var model = new RatPack { FirstName = "Andy" };
            //    return Response.AsXml(model);
            //};

        }

        

        
    }
}