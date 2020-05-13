using System;
using System.Collections.Generic;
using System.Net;
using HttpMockSlim.Handlers;
using HttpMockSlim.Handlers.Internal;
using HttpMockSlim.HttpListener;
using HttpMockSlim.Model;

namespace HttpMockSlim
{
    public class HttpMock : IDisposable
    {
        #region Fields

        private readonly IHttpServer _httpServer;
        private readonly List<IHttpHandlerMock> _handlers;
        private readonly IHttpHandlerMock _defaultHandler = new NotFoundResponseHandler();

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

        // TODO: Do these Add's really belong here? maybe it should be attached by extension or smth?

        public HttpMock Add(string method, string relativePath, Action<Request, Response> responseFiller)
        {
            var realHandler = new SimpleFuncHandler((request, response) =>
            {
                responseFiller(request, response);
                return true;
            });

            return Add(new FilteredHandlerWrapper(method, relativePath, realHandler));
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
                _defaultHandler.Handle(context);
            }
        }

        private bool TryHandle(HttpListenerContext context)
        {
            bool handled = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            // Doing conversion breaks first-added-first-tested order
            try
            {
                for (int i = 0; i < _handlers.Count; i++)
                {
                    if (_handlers[i].Handle(context))
                    {
                        handled = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());

                context.Response.Close();
                handled = true;
            }

            return handled; 
        }

        private static bool SafeHandle(IHttpHandlerMock handler, HttpListenerContext context)
        {
            try
            {
                return handler.Handle(context);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _httpServer.Stop();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}