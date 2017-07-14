using System.Net;
using HttpMockSlim.Utils;

namespace HttpMockSlim.Playground.Handlers
{
    public class LowLevelExample : IHttpHandlerMock
    {
        // Lets handle all DELETE's
        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod != "DELETE")
                return false;

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";

            new StreamGenerator(256, 'A').CopyTo(context.Response.OutputStream);

            // One must not forget to close it, as .NET can decide to send the data only after Close().
            context.Response.Close();

            return true;
        }
    }
}
