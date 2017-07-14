using System;
using System.Net;

namespace HttpMockSlim.Handlers
{
    public abstract class FilteredHandlerBase : IHttpHandlerMock
    {
        private readonly string _method;
        private readonly string _path;

        public StringComparison ComparsionType { get; set; } = StringComparison.InvariantCulture;

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

            if (request.HttpMethod.Equals(_method, ComparsionType) &&
                request.RawUrl.Equals(_path, ComparsionType))
            {
                result = HandleInner(context);
            }

            return result;
        }

        protected abstract bool HandleInner(HttpListenerContext context);
    }
}