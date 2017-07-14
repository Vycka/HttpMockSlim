using System;
using HttpMockSlim.LoadTest.Client.Data;

namespace HttpMockSlim.LoadTest.Client.Utils
{
    public class VerbosePrinter
    {
        public void Attach(SimpleWebRequest webRequest)
        {
            webRequest.BeforeRequest += WebRequestOnBeforeRequest;
            webRequest.AfterRequest += WebRequestOnAfterRequest;
        }

        private void WebRequestOnAfterRequest(SimpleWebRequest sender, RequestResult result)
        {
            Console.WriteLine("##");
        }

        private void WebRequestOnBeforeRequest(SimpleWebRequest sender, RequestData request)
        {
            Console.WriteLine(" # API {2} [{0}] [{1}]", sender, request.Url, request.Method);

            string trimmedRequest = request.Data.SmartTrim(200);

            if (!String.IsNullOrWhiteSpace(trimmedRequest))
            {
                Console.WriteLine(" # REQ -> [{0}]", trimmedRequest);
            }
        }
    }
}