using System;
using HttpMockSlim.LoadTest.Client.Enums;

namespace HttpMockSlim.LoadTest.Client.Data
{
    public class RequestData
    {
        public Uri Url;
        public RequestMethod Method;
        public string Data;

        public RequestData(Uri requestUrl, RequestMethod requestMethod, string requestData)
        {
            Url = requestUrl;
            Method = requestMethod;
            Data = requestData;
        }
    }
}