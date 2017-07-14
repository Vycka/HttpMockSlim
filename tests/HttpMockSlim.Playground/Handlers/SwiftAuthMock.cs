using System;
using System.Net;
using HttpMockSlim.Handlers;

namespace HttpMockSlim.Playground.Handlers
{
    public class SwiftAuthMock : FilteredHandlerBase
    {
        private readonly string _fakeSwiftPath;

        public SwiftAuthMock(string fakeSwiftPath) : base("GET", "/auth")
        {
            if (fakeSwiftPath == null)
                throw new ArgumentNullException(nameof(fakeSwiftPath));
                
            _fakeSwiftPath = fakeSwiftPath;
        }

        protected override bool HandleInner(HttpListenerContext context)
        {
            // No idea if this response is valid for Swift RFC. but it works :)

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";

            context.Response.SendChunked = true;
            context.Response.AddHeader("X-Auth-Token", "fifty_shades_of_mocked_passkey");
            context.Response.AddHeader("X-Storage-Url", $"{context.Request.Url.Scheme}://{context.Request.Url.Authority}{_fakeSwiftPath}");

            context.Response.Close();

            return true;
        }
    }
}
