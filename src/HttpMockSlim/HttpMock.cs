using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HttpMockSlim.Handlers;
using HttpMockSlim.HttpListener;
using HttpMockSlim.Model;

namespace HttpMockSlim
{
    public class HttpMock : IDisposable
    {
        #region Fields

        private readonly IHttpServer _httpServer;
        private readonly List<IHttpHandlerMock> _handlers;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes HttpMock library with default implementation
        /// </summary>
        public HttpMock() : this(new HttpListenerServer())
        {
        }

        /// <summary>
        /// Initializes HttpMock library with custom IHttpServer
        /// </summary>
        /// <param name="httpServer">Http server implementation to use</param>
        public HttpMock(IHttpServer httpServer)
        {
            if (httpServer == null)
                throw new ArgumentNullException(nameof(httpServer));

            _httpServer = httpServer;
            _handlers = new List<IHttpHandlerMock>();
        }

        #endregion

        #region Properties

        public bool IsRunning => _httpServer.IsRunning;
        
        #endregion

        #region Exposed Start/Stop

        public void Start(string uriPrefix = "http://localhost:8080/")
        {
            if (IsRunning)
                throw new InvalidOperationException("Server is already running");

            _httpServer.Start(uriPrefix, SessionReceived);
        }

        public void Stop()
        {
            if (!IsRunning)
                throw new InvalidOperationException("Server is not running running");

            _httpServer.Stop();
        }

        #endregion

        #region Public handlers setup

        public HttpMock Add(string method, string relativePath, Action<Request, Response> responseFiller)
        {
            Func<Request, Response, bool> handlerFunc = (request, response) =>
            {
                bool handled = false;

                if (request.RawUrl.Equals(relativePath, StringComparison.InvariantCulture) &&
                    request.Method.Equals(method, StringComparison.InvariantCultureIgnoreCase))
                {
                    responseFiller(request, response);

                    handled = true;
                }

                return handled;
            };

            SimpleFuncHandler handler = new SimpleFuncHandler(handlerFunc);

            return Add(handler);
        }

        public HttpMock Add(Action<Request, Response> responseFiller)
        {
            return Add(new SimpleFuncHandler((req, resp) => {
                responseFiller(req, resp);
                return true;
            }));
        }

        public HttpMock Add(IHttpHandlerMock handlerMock)
        {
            _handlers.Add(handlerMock);

            return this;
        }

        #endregion

        #region Handle

        private void SessionReceived(HttpListenerContext context)
        {
            if (!TryHandle(context))
            {
                var notFoundMessage = new MemoryStream(Encoding.UTF8.GetBytes($"Can't find any handlers for this request"));

                context.Response.StatusCode = 404;
                notFoundMessage.CopyTo(context.Response.OutputStream);

                context.Response.Close();
            }
        }

        private bool TryHandle(HttpListenerContext context)
        {
            bool handled = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            // Doing conversion breaks first-added-first-tested order
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].Handle(context))
                {
                    handled = true;
                    break;
                }
            }

            return handled; 
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _httpServer.Stop();
        }

        #endregion
    }
}