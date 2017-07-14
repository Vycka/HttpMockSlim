using System;
using System.Net;

namespace HttpMockSlim.Extensions
{
    public static class HttpListenerRequestExtensions
    {
        public static bool IsGZipped(this HttpListenerRequest request)
        {
            string content = request.Headers["Content-Encoding"];
            return content.StartsWith("gzip", StringComparison.InvariantCulture);
        }
    }
}