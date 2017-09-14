using System;
using System.Net;
using HttpMockSlim.LoadTest.Client.Data;
using HttpMockSlim.LoadTest.Client.Utils;

namespace HttpMockSlim.LoadTest.Client.Exceptions
{
    public class ExtendedWebException : Exception
    {
        public RequestResult RequestResult;
        public readonly HttpMockSlim.LoadTest.Client.SimpleWebRequest WebRequest;

        public ExtendedWebException(WebException webException, RequestResult requestResult, HttpMockSlim.LoadTest.Client.SimpleWebRequest webRequest)
            : base(webException.Message, webException)
        {
            WebRequest = webRequest;
            RequestResult = requestResult;
        }

        // Overrided message property, just for easier debugging
        public override string Message => ToString();

        public override string ToString()
        {
            return ErrorFormatter.FormatError(WebRequest, RequestResult, base.Message); 
        }
    }
}