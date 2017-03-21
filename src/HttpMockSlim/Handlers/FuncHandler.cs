using System;
using System.Net;

namespace HttpMockSlim.Handlers
{
    public class FuncHandler : IHttpHandlerMock
    {
        private readonly Func<HttpListenerContext, bool> _handlerFunc;

        public FuncHandler(Func<HttpListenerContext, bool> handlerFunc)
        {
            if (handlerFunc == null)
                throw new ArgumentNullException(nameof(handlerFunc));

            _handlerFunc = handlerFunc;
        }

        public bool Handle(HttpListenerContext context)
        {
            return _handlerFunc(context);
        }
    }
}