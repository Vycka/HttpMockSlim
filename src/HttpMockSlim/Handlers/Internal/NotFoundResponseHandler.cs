using System.IO;
using System.Text;

namespace HttpMockSlim.Handlers.Internal
{
    internal class NotFoundResponseHandler : SimpleFuncHandler
    {
        private static readonly string _message = "Can't find any handlers for this request";

        public NotFoundResponseHandler() : base((request, response) =>
        {
            new MemoryStream(Encoding.UTF8.GetBytes(_message)).CopyTo(response.Body);
            response.StatusCode = 404;

            return true;
        })
        {
        }
    }
}