using System.IO;
using System.Net;
using HttpMockSlim.Utils;

namespace HttpMockSlim.Playground.Handlers
{
    public class HandleAll : IHttpHandlerMock
    {
        public bool Handle(HttpListenerContext context)
        {
            string all = new StreamReader(context.Request.InputStream).ReadToEnd();
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";

            new StreamGenerator(256, 'A').CopyTo(context.Response.OutputStream);

            // One must not forget to close it, as .NET can decide to send the data only after Close().
            context.Response.Close();

            return true;
        }
    }
}