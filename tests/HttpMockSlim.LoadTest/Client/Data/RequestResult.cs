using System;
using System.Text;
using HttpMockSlim.LoadTest.Client.Utils;

namespace HttpMockSlim.LoadTest.Client.Data
{
    public class RequestResult
    {
        public RequestData Request;
        public ResponseData Response;

        public RequestResult(RequestData requestData, ResponseData responseData)
        {
            Request = requestData;
            Response = responseData;
        }

        private string _cachedFormatedMetadata;

        private string FormatedMetadata
        {
            get
            {
                if (_cachedFormatedMetadata == null)
                    _cachedFormatedMetadata = FormatMetadataString(1024);

                return _cachedFormatedMetadata;
            }
        }
        public string FormatMetadataString(int printResponseDataLength)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("-----------------------\r\n");
            sb.AppendFormat("Request:\r\n");
            sb.AppendFormat(" * Url:     {0}\r\n", Request.Url);
            sb.AppendFormat(" * Method:  {0}\r\n", Request.Method);
            if (!String.IsNullOrWhiteSpace(Request.Data))
                sb.AppendFormat(" * Data:    {0}\r\n", Request.Data);

            if (Response != null && (Response.Code != null || Response.Data != null || Response.Url != null))
            {
                sb.AppendFormat("Response:\r\n");

                if (Response.Url != null)
                    sb.AppendFormat(" * Url:     {0}\r\n", Response.Url);

                if (Response.Code != null)
                    sb.AppendFormat(" * Code:    {0} [{1}]\r\n", (int)Response.Code, Response.Code);

                if (Response.Data != null)
                    sb.AppendFormat(" * Data:    {0}\r\n", Response.Data.SmartTrim(printResponseDataLength));
            }

            sb.Append("-----------------------");

            return sb.ToString();
        }

        public override string ToString()
        {
            return FormatedMetadata;
        }
    }
}