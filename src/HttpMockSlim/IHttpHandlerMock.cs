using HttpMockSlim.Model;

namespace HttpMockSlim
{
    public interface IHttpHandlerMock
    {
        bool Handle(Request request, Response response);
    }
}