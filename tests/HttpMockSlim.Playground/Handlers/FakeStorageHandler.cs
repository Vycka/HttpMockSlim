using System;
using System.IO;
using System.Net;
using HttpMockSlim.Extensions;
using HttpMockSlim.Utils;

namespace HttpMockSlim.Playground.Handlers
{
    public class FakeStorageHandler : IHttpHandlerMock
    {
        protected readonly string PathBase;

        public FakeStorageHandler(string pathBase)
        {
            if (pathBase == null)
                throw new ArgumentNullException(nameof(pathBase));

            PathBase = pathBase;
        }

        public bool Handle(HttpListenerContext context)
        {
            bool handled = false;
            HttpListenerRequest request = context.Request;

            if (request.RawUrl.StartsWith(PathBase, StringComparison.InvariantCulture))
            {
                handled = true;

                switch (request.HttpMethod)
                {
                    case "GET":
                        HandleGet(context);
                        break;

                    case "PUT":
                        HandlePut(context);
                        break;

                    case "DELETE":
                        HandleDelete(context);
                        break;

                    default:
                        handled = false;
                        break;
                }
            }

            return handled;
        }

        protected virtual void HandleGet(HttpListenerContext context)
        {
            WriteResponse(context, new StreamGenerator(16, '#'));
        }

        protected virtual void HandlePut(HttpListenerContext context)
        {
            // Don't forget to all, what was sent anyway
            var result = context.Request.InputStream.ReadAllBytes();

            WriteResponse(context, new StreamGenerator(0, 0));
        }

        protected virtual void HandleDelete(HttpListenerContext context)
        {
            WriteResponse(context, new StreamGenerator(0, 0));
        }

        protected static void WriteResponse(HttpListenerContext context, int statusCode)
        {
            HttpListenerResponse response = context.Response;

            response.StatusCode = statusCode;
            response.ContentType = "text/plain";
            response.SendChunked = true;

            response.Close();
        }

        protected static void WriteResponse(HttpListenerContext context, Stream body)
        {
            HttpListenerResponse response = context.Response;

            response.StatusCode = 200;
            response.ContentType = "text/plain";
            response.SendChunked = true;

            body.CopyTo(response.OutputStream);

            response.Close();
        }
    }
}