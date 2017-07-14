using System;
using System.IO;
using System.Net;
using HttpMockSlim.Model;

namespace HttpMockSlim.Handlers
{
    public class SimpleFuncHandler : IHttpHandlerMock
    {
        private readonly Func<Request, Response, bool> _funcHandler;

        public SimpleFuncHandler(Func<Request,Response,bool> funcHandler)
        {
            if (funcHandler == null)
                throw new ArgumentNullException(nameof(funcHandler));

            _funcHandler = funcHandler;
        }

        public bool Handle(HttpListenerContext context)
        {
            var request = MapRequest(context.Request);
            Response response = new Response();

            bool handled = _funcHandler(request, response);


            if (handled)
            {
                WriteResponse(context.Response, response);
            }

            return handled;
        }

        private static Request MapRequest(HttpListenerRequest clientRequest)
        {
            Request result = new Request
            {
                Method = clientRequest.HttpMethod,
                RawUrl = clientRequest.RawUrl
            };

            if (clientRequest.HasEntityBody)
            {
                using (Stream body = clientRequest.InputStream)
                using (StreamReader reader = new StreamReader(body, clientRequest.ContentEncoding))
                {
                    result.Body = reader.ReadToEnd();
                }
            }

            return result;
        }

        private static void WriteResponse(HttpListenerResponse httpResponse, Response response)
        {
            httpResponse.StatusCode = response.StatusCode;
            httpResponse.ContentType = response.ContentType;
            httpResponse.SendChunked = true;
            response.Body.CopyTo(httpResponse.OutputStream);

            httpResponse.Close();
        }
    }
}