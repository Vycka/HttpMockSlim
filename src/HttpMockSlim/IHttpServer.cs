using System;
using HttpMockSlim.Model;

namespace HttpMockSlim
{
    public interface IHttpServer
    {
        bool IsRunning { get; }

        void Start(string uriPrefix, Action<Request, Response> sessionReceived);

        void Stop();
    }
}