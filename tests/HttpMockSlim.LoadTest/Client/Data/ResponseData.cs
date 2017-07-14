using System;
using System.Collections.Generic;
using System.Net;

namespace HttpMockSlim.LoadTest.Client.Data
{
    public class ResponseData
    {

        public Uri Url;
        public HttpStatusCode? Code;
        public IReadOnlyDictionary<string, string> Headers;
        public readonly string Data;

        public ResponseData(Uri responseUrl, HttpStatusCode? responseCode, Dictionary<string,string> headers, string responseData)
        {
            Url = responseUrl;
            Code = responseCode;
            Headers = headers;

            Data = responseData;
        }

    }
}