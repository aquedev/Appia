﻿namespace Nancy.Hosting.Self
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

        public NancyHost(Uri baseUri)
            : this(baseUri, NancyBootstrapperLocator.Bootstrapper)
        {
        }

        public NancyHost(Uri baseUri, INancyBootstrapper bootStrapper)
        {
            this.baseUri = baseUri;
            listener = new HttpListener();
            listener.Prefixes.Add(baseUri.ToString());

            bootStrapper.Initialise();
            engine = bootStrapper.GetEngine();
        }

        public void Start()
        {
            shouldContinue = true;

            listener.Start();
            thread = new Thread(Listen);
            thread.Start();
        }

        public void Stop()
        {
            shouldContinue = false;
            listener.Stop();
        }

        private void Listen()
        {
            while (shouldContinue)
            {
                HttpListenerContext requestContext;
                try
                {
                    requestContext = listener.GetContext();
                }
                catch (HttpListenerException)
                {
                    // this will be thrown when listener is closed while waiting for a request
                    return;
                }
                var nancyRequest = ConvertRequestToNancyRequest(requestContext.Request);
                using (var nancyContext = engine.HandleRequest(nancyRequest))
                {
                    ConvertNancyResponseToResponse(nancyContext.Response, requestContext.Response);
                }
            }
        }

        private static Uri GetUrlAndPathComponents(Uri uri) 
        {
            // ensures that for a given url only the
            //  scheme://host:port/paths/somepath
            return new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
        }

        private Request ConvertRequestToNancyRequest(HttpListenerRequest request)
        {
            var relativeUrl = 
                GetUrlAndPathComponents(baseUri).MakeRelativeUri(GetUrlAndPathComponents(request.Url));

            var expectedRequestLength =
                GetExpectedRequestLength(request.Headers.ToDictionary());

            return new Request(
                request.HttpMethod,
                string.Concat("/", relativeUrl),
                request.Headers.ToDictionary(),
                RequestStream.FromStream(request.InputStream, expectedRequestLength, true),
                request.Url.Scheme,
                request.Url.Query);
        }

        private static long GetExpectedRequestLength(IDictionary<string, IEnumerable<string>> incomingHeaders)
        {
            if (incomingHeaders == null)
            {
                return 0;
            }

            if (!incomingHeaders.ContainsKey("Content-Length"))
            {
                return 0;
            }

            var headerValue =
                incomingHeaders["Content-Length"].SingleOrDefault();

            if (headerValue == null)
            {
                return 0;
            }

            long contentLength;
            if (!long.TryParse(headerValue, NumberStyles.Any, CultureInfo.InvariantCulture, out contentLength))
            {
                return 0;
            }

            return contentLength;
        }

        private static void ConvertNancyResponseToResponse(Response nancyResponse, HttpListenerResponse response)
        {
            foreach (var header in nancyResponse.Headers)
            {
                response.AddHeader(header.Key, header.Value);
            }

            foreach (var nancyCookie in nancyResponse.Cookies)
            {
                response.Cookies.Add(ConvertCookie(nancyCookie));
            }

            response.ContentType = nancyResponse.ContentType;
            response.StatusCode = (int)nancyResponse.StatusCode;

            using (var output = response.OutputStream)
            {
                nancyResponse.Contents.Invoke(output);
            }
        }

        private static Cookie ConvertCookie(INancyCookie nancyCookie)
        {
            var cookie = 
                new Cookie(nancyCookie.Name, nancyCookie.Value, nancyCookie.Path, nancyCookie.Domain);

            if (nancyCookie.Expires.HasValue)
            {
                cookie.Expires = nancyCookie.Expires.Value;
            }

            return cookie;
        }
    }
}