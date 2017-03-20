using System;
using System.Net;
using HttpMockSlim.Model;

namespace HttpMockSlim
{
    public interface IHttpServer
    {
        bool IsRunning { get; }

        void Start(string uriPrefix, Action<HttpListenerContext> sessionReceived);

        void Stop();
    }
}