namespace Nancy.Hosting.Self
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Linq;
    using IO;
    using Nancy.Bootstrapper;
    using Nancy.Cookies;
    using Nancy.Extensions;
    using HttpListener = HttpServer.HttpListener;
using HttpServer;
    using HttpServer.Headers;
    using HttpServer.Messages;
    using System.IO;
    using System.Text;
using HttpServer.Modules;
    using HttpServer.Logging;

    /// <summary>
    /// Allows to host Nancy server inside any application - console or windows service.
    /// </summary>
    /// <remarks>
    /// NancyHost uses <see cref="System.Net.HttpListener"/> internally. Therefore, it requires full .net 4.0 profile (not client profile)
    /// to run. <see cref="Start"/> will launch a thread that will listen for requests and then process them. All processing is done
    /// within a single thread - self hosting is not intended for production use, but rather as a development server.
    /// </remarks>
    public class NancyHost  
    {
        private readonly Uri baseUri;
        private readonly Server server;
        private readonly INancyEngine engine;
        private bool shouldContinue;

        public NancyHost(IPAddress address, int port)
            : this(address, port, NancyBootstrapperLocator.Bootstrapper)
        {
        }

        public NancyHost(IPAddress address, int port, INancyBootstrapper bootStrapper)
        {
            var filter = new LogFilter();
            filter.AddStandardRules();
            LogFactory.Assign(new ConsoleLogFactory(filter));
            server = new Server();
            
            bootStrapper.Initialise();
            engine = bootStrapper.GetEngine();

            // same as previous example.
            AppiaModule module = new AppiaModule(engine);
            server.Add(module);
        }

        public void Start()
        {
           

            // use one http listener.
            server.Add(HttpListener.Create(IPAddress.Any, 8888));
            

            shouldContinue = true;
            server.Start(5);
        }

        public void Stop()
        {
            shouldContinue = false;
            server.Stop(true);
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {

            try
            {
                e.Response.Connection.Type = ConnectionType.Close;

                //e.Response.Connection.Type = ConnectionType.Close;
                   
                using (StreamWriter writer = new StreamWriter(e.Response.Body, Encoding.UTF8))
                {
                    writer.WriteLine("<h1>Processing {0}</h1>", e.Request.Uri);
                    writer.Flush();
                } 
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter(e.Response.Body))
                {
                    writer.WriteLine("<h1>Error while rendering {0}</h1>", e.Request.Uri);
                    writer.WriteLine("<hr/>");
                    writer.WriteLine(ex.Message);
                    writer.WriteLine(ex.StackTrace.Replace(Environment.NewLine, "<br/>"));
                }
                Console.WriteLine("Error executing request for " + e.Request.Uri);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private Uri GetUrlAndPathComponents(Uri uri) 
        {
            // ensures that for a given url only the
            //  scheme://host:port/paths/somepath
            return new Uri(uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));
        }

        

        
    }

    public class AppiaModule : IModule
    {
        private INancyEngine engine;

        public AppiaModule(INancyEngine engine)
        {
            this.engine = engine;
        }
        public ProcessingResult  Process(RequestContext context)
        {
            IRequest request = context.Request;
            IResponse response = context.Response;

            var nancyRequest = ConvertRequestToNancyRequest(request);
            using (var nancyContext = engine.HandleRequest(nancyRequest))
            {
                using (MemoryStream mstream = new MemoryStream())
                {
                    nancyContext.Response.Contents.Invoke(mstream);

                    response.ContentType.Value = nancyContext.Response.ContentType;
                    response.ContentLength.Value = mstream.Length;
                    var generator = HttpFactory.Current.Get<ResponseWriter>();
                    generator.SendHeaders(context.HttpContext, response);
                    generator.SendBody(context.HttpContext, mstream);
                }
            }
            return ProcessingResult.Abort;
        }

        private Request ConvertRequestToNancyRequest(IRequest request)
        {
            var expectedRequestLength =
                GetExpectedRequestLength(request.Headers);

            return new Request(
                request.Method,
                request.Uri.LocalPath,
                ConvertToNancyHeaders(request.Headers),
                RequestStream.FromStream(request.Body, expectedRequestLength, true),
                request.Uri.Scheme,
                request.Uri.Query);
        }

        private IDictionary<string, IEnumerable<string>> ConvertToNancyHeaders(IHeaderCollection headers)
        {
            Dictionary<string, IEnumerable<string>> dict = new Dictionary<string, IEnumerable<string>>();
            foreach (var header in headers)
            {
                if (dict.ContainsKey(header.Name))
                    ((List<string>)dict[header.Name]).Add(header.HeaderValue);
                else if (string.Equals(header.Name, "cookie", StringComparison.CurrentCultureIgnoreCase) == false)
                    dict.Add(header.Name, new List<string> { header.HeaderValue });
            }
            return dict;
        }

        private long GetExpectedRequestLength(IHeaderCollection incomingHeaders)
        {
            if (incomingHeaders == null)
            {
                return 0;
            }

            var headersDict = incomingHeaders.ToDictionary(x => x.Name);

            if (!headersDict.ContainsKey("Content-Length"))
            {
                return 0;
            }

            var headerValue =
                headersDict["Content-Length"];

            if (headerValue == null)
            {
                return 0;
            }

            long contentLength;
            if (!long.TryParse(headerValue.HeaderValue, NumberStyles.Any, CultureInfo.InvariantCulture, out contentLength))
            {
                return 0;
            }

            return contentLength;
        }

        
    }
}