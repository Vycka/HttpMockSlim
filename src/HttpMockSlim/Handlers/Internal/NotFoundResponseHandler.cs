using System.IO;
using System.Text;

namespace HttpMockSlim.Handlers.Internal
{
    internal class NotFoundResponseHandler : SimpleFuncHandler
    {
        private static readonly byte[] _message = Encoding.UTF8.GetBytes("Can't find any handlers for this request");

        public NotFoundResponseHandler() : base((request, response) =>
        {
            response.Body = new MemoryStream(_message);
            response.StatusCode = 404;
            return true;
        })
        {
        }
    }
}