using System.Net;
using HttpMockSlim.Model;

namespace HttpMockSlim
{
    public interface IHttpHandlerMock
    {
        bool Handle(HttpListenerContext context);
    }
}