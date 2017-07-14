using System;
using System.Net;

namespace HttpMockSlim.Handlers
{
    /// <summary>
    /// Handler wrapper: wraps HTTP Method & Path filtering to around provided [realHandler]
    /// </summary>
    public class FilteredHandlerWrapper : FilteredHandlerBase
    {
        private readonly IHttpHandlerMock _realHandler;

        /// <summary>
        /// Setup new instance
        /// </summary>
        /// <param name="method">HTTP method to pass through</param>
        /// <param name="path">HTTP request path to pass through</param>
        /// <param name="realHandler">Real handler which gets called if method & path fillters pass</param>
        public FilteredHandlerWrapper(string method, string path, IHttpHandlerMock realHandler) : base(method, path)
        {
            if (realHandler == null)
                throw new ArgumentNullException(nameof(realHandler));

            _realHandler = realHandler;
        }

        protected override bool HandleInner(HttpListenerContext context)
        {
            return _realHandler.Handle(context);
        }
    }
}