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
        private readonly HttpListener listener;
        private readonly INancyEngine engine;
        private Thread thread;
        private bool shouldContinue;

        public NancyHost(IPAddress address, int port)
            : this(address, port, NancyBootstrapperLocator.Bootstrapper)
        {
        }

        public NancyHost(IPAddress address, int port, INancyBootstrapper bootStrapper)
        {
            HttpListener listener = HttpListener.Create(address, port);
            bootStrapper.Initialise();
            engine = bootStrapper.GetEngine();
            baseUri = new Uri(String.Format("http://{0}:{1}/", address, port));
        }

        public void Start()
        {
            shouldContinue = true;
            listener.Start(5);
        }

        public void Stop()
        {
            shouldContinue = false;
            listener.Stop();
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            e.Response.Connection.Type = ConnectionType.Close;
            var nancyRequest = ConvertRequestToNancyRequest(e.Request);
            using (var nancyContext = engine.HandleRequest(nancyRequest))
            {
                ConvertNancyResponseToResponse(nancyContext.Response, e.Response);
            }
        }

        private Uri GetUrlAndPathComponents(Uri uri) 
        {
            // ensures that for a given url only the
            //  scheme://host:port/paths/somepath
            return new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
        }

        private Request ConvertRequestToNancyRequest(IRequest request)
        {
            var relativeUrl = 
                GetUrlAndPathComponents(baseUri).MakeRelativeUri(GetUrlAndPathComponents(request.Uri));

            var expectedRequestLength =
                GetExpectedRequestLength(request.Headers);

            return new Request(
                request.Method,
                string.Concat("/", relativeUrl),
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
                else
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

        private void ConvertNancyResponseToResponse(Response nancyResponse, IResponse response)
        {
            foreach (var header in nancyResponse.Headers)
            {
                response.Add(new StringHeader(header.Key, header.Value));
            }

            foreach (var nancyCookie in nancyResponse.Cookies)
            {
                response.Cookies.Add(ConvertCookie(nancyCookie));
            }

            response.ContentType = new ContentTypeHeader(nancyResponse.ContentType);
            response.Status = (HttpStatusCode)nancyResponse.StatusCode;

            using (var output = response.Body)
            {
                nancyResponse.Contents.Invoke(output);
            }
        }

        private ResponseCookie ConvertCookie(INancyCookie nancyCookie)
        {
            DateTime expires = DateTime.Now.AddDays(1);
            if (nancyCookie.Expires.HasValue)
            {
                expires = nancyCookie.Expires.Value;
            }

            var cookie = 
                new ResponseCookie(nancyCookie.Name, nancyCookie.Value, expires, nancyCookie.Path, nancyCookie.Domain);

            return cookie;
        }
    }
}