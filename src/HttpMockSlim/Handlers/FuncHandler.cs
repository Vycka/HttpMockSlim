using System;
using HttpMockSlim.Model;

namespace HttpMockSlim.Handlers
{
    public class FuncHandler : IHttpHandlerMock
    {
        private readonly Func<Request, Response, bool> _funcHandler;

        public FuncHandler(Func<Request,Response,bool> funcHandler)
        {
            if (funcHandler == null)
                throw new ArgumentNullException(nameof(funcHandler));
            _funcHandler = funcHandler;
        }

        public bool Handle(Request request, Response response)
        {
            return _funcHandler(request, response);
        }
    }
}