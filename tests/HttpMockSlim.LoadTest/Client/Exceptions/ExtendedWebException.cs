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

        // Overrided message property, because it's only used in Adform.Reporting.QA atm, and doing workaround here is just quicker for the time being.
        public override string Message => ToString();

        public override string ToString()
        {
            return ErrorFormatter.FormatError(WebRequest, RequestResult, base.Message); 
        }
    }
}