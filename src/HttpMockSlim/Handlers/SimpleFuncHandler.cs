﻿using System;
using System.IO;
using System.Net;
using HttpMockSlim.Extensions;
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
                RawUrl = clientRequest.RawUrl,
                Headers = clientRequest.Headers
            };

            if (clientRequest.HasEntityBody)
            {
                Stream requestStream = clientRequest.InputStream;

                if (requestStream != null)
                {
                    if (clientRequest.IsGZipped())
                        requestStream = requestStream.DecompressGZip();

                    result.Body = requestStream?.ReadAll();
                }
            }

            return result;
        }

        private static void WriteResponse(HttpListenerResponse httpResponse, Response response)
        {
            httpResponse.StatusCode = response.StatusCode;
            httpResponse.ContentType = response.ContentType;
            httpResponse.SendChunked = true;
            if (response.Headers != null)
            {
                foreach (string headerKey in response.Headers.AllKeys)
                {
                    httpResponse.Headers.Add(headerKey, response.Headers[headerKey]);
                }
            }
            response.Body.CopyTo(httpResponse.OutputStream);

            httpResponse.Close();
        }
    }
}