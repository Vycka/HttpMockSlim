using System.IO;
using System.Net;
using System.Text;
using HttpMockSlim.Utils;

namespace HttpMockSlim.Playground.Handlers
{
    public class LowLevelExample : IHttpHandlerMock
    {
        // Includes stream Generator demo
        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.RawUrl != "/stream")
                return false;

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            MemoryStream prefixStream = new MemoryStream(Encoding.UTF8.GetBytes("\""));
            StreamGenerator longStream = new StreamGenerator(1024, 'A');
            MemoryStream suffixStream = new MemoryStream(Encoding.UTF8.GetBytes("\""));

            CombinedStream combinedStream = new CombinedStream(new Stream[] { prefixStream, longStream, suffixStream });

            combinedStream.CopyTo(context.Response.OutputStream);
            // One must not forget to close it, as .NET can decide to send the data only after Close().
            context.Response.Close();

            return true;
        }
    }
}
