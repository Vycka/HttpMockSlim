using System;
using System.Net;

namespace HttpMockSlim.Handlers
{
    public abstract class FilteredHandlerBase : IHttpHandlerMock
    {
        private readonly string _method;
        private readonly string _path;

        protected FilteredHandlerBase(string method, string path)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            _method = method;
            _path = path;
        }

        public bool Handle(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            bool result = false;

            if (request.HttpMethod.Equals(_method, StringComparison.InvariantCulture) &&
                request.RawUrl.Equals(_path, StringComparison.InvariantCulture))
            {
                HandleInner(context);

                result = true;
            }

            return result;
        }

        protected abstract void HandleInner(HttpListenerContext context);
    }
}