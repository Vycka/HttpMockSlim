using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HttpMockSlim.Extensions;
using HttpMockSlim.LoadTest.Client.Data;
using HttpMockSlim.LoadTest.Client.Enums;
using HttpMockSlim.LoadTest.Client.Exceptions;

namespace HttpMockSlim.LoadTest.Client
{
    public class SimpleWebRequest
    {
        #region Fields

        protected readonly CookieContainer _sessionCookies;

        public string Name { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Timeout in milliseconds.
        /// </summary>
        public int Timeout = 60000;

        #endregion

        #region Constructors

        public SimpleWebRequest(string name = @"N\A")
        {
            _sessionCookies = new CookieContainer();

            Name = name;
        }

        #endregion

        #region Exposed Requests

        public RequestResult Execute(string baseUrl, string relativeUrl, RequestMethod requestMethod, params Header[] headers)
        {
            HttpWebRequest httpRequest = BuildWebRequest(new Uri(new Uri(baseUrl), relativeUrl), requestMethod);
            InsertHeaders(httpRequest, headers);
            return ExecuteWebRequest(httpRequest, null);
        }
        public RequestResult Execute(string baseUrl, string relativeUrl, RequestMethod requestMethod, SubmitRequestType submitRequestType, byte[] submitRequestData, params Header[] headers)
        {
            HttpWebRequest httpRequest = BuildWebRequest(new Uri(new Uri(baseUrl), relativeUrl), requestMethod);
            InsertHeaders(httpRequest, headers);
            WriteRequestBody(httpRequest, submitRequestType, submitRequestData);
            return ExecuteWebRequest(httpRequest, submitRequestData);
        }
        public RequestResult Execute(Uri requestAbsoluteUrl, RequestMethod requestMethod, params Header[] headers)
        {
            HttpWebRequest httpRequest = BuildWebRequest(requestAbsoluteUrl, requestMethod);
            InsertHeaders(httpRequest, headers);
            return ExecuteWebRequest(httpRequest, null);
        }

        public RequestResult Execute(Uri requestAbsoluteUrl, RequestMethod requestMethod, SubmitRequestType submitRequestType, byte[] submitRequestData, params Header[] headers)
        {
            HttpWebRequest httpRequest = BuildWebRequest(requestAbsoluteUrl, requestMethod);
            InsertHeaders(httpRequest, headers);
            WriteRequestBody(httpRequest, submitRequestType, submitRequestData);
            return ExecuteWebRequest(httpRequest, submitRequestData);
        }

        #endregion

        #region Generic HTTP Request utils

        /// <summary>
        /// Executes Provided Http Request
        /// </summary>
        /// <param name="httpRequest">Http Request</param>
        /// <param name="refRequestData">Used only for filling RequestData.Request.Data in hacky way</param>
        /// <returns>Response Data</returns>
        protected virtual RequestResult ExecuteWebRequest(HttpWebRequest httpRequest, byte[] refRequestData)
        {
            RequestData requestData = new RequestData(
                httpRequest.RequestUri,
                (RequestMethod)Enum.Parse(typeof(RequestMethod), httpRequest.Method, true),
                new MemoryStream(refRequestData).ReadAll()
            );
            ResponseData responseData = null;
            RequestResult result = null;

            OnBeforeRequestEvent(requestData);

            try
            {
                using (WebResponse webResponse = httpRequest.GetResponse())
                {
                    Dictionary<string, string> headers = ExtractHeaders(webResponse);
                    using (Stream httpStream = webResponse.GetResponseStream())
                    using (StreamReader httpResponseReader = new StreamReader(httpStream, Encoding.UTF8))
                    {

                        string responseString = httpResponseReader.ReadToEnd();

                        responseData = new ResponseData(
                            webResponse.ResponseUri,
                            ((HttpWebResponse)webResponse).StatusCode,
                            headers,
                            responseString
                        );
                    }
                }

                result = new RequestResult(requestData, responseData);
                OnAfterRequestEvent(result);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    {
                        string responseString = "";

                        if (responseStream != null)
                        {
                            using (StreamReader httpResponseReader = new StreamReader(responseStream, Encoding.UTF8))
                            {
                                responseString = httpResponseReader.ReadToEnd();
                            }
                        }

                        responseData = new ResponseData(
                            ex.Response.ResponseUri,
                            ((HttpWebResponse)ex.Response).StatusCode,
                            ExtractHeaders(ex.Response),
                            responseString
                        );
                    }
                }

                result = new RequestResult(requestData, responseData);
                OnAfterRequestEvent(result);

                throw new ExtendedWebException(ex, new RequestResult(requestData, responseData), this);
            }

            return result;
        }

        private static Dictionary<string, string> ExtractHeaders(WebResponse webResponse)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(webResponse.Headers.Count);
            foreach (string key in webResponse.Headers)
            {
                headers[key] = webResponse.Headers[key];
            }

            return headers;
        }

        /// <summary>
        /// Inserts headers into HttpRequest
        /// </summary>
        /// <param name="httpRequest">Target HttpRequest</param>
        /// <param name="headers">headers to insert</param>
        private void InsertHeaders(HttpWebRequest httpRequest, Header[] headers)
        {
            if (headers != null && headers.Length > 0)
            {
                foreach (Header header in headers)
                {
                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }
        }

        /// <summary>
        /// Inserts request stream into HttpRequest
        /// </summary>
        /// <param name="httpRequest">HttpRequest to which stream will be inserted</param>
        /// <param name="requestContent">Request Data</param>
        /// <param name="requestType">Type of Request</param>
        private void WriteRequestBody(HttpWebRequest httpRequest, SubmitRequestType requestType, byte[] requestBytes)
        {
            httpRequest.SendChunked = true;

            switch (requestType)
            {
                case SubmitRequestType.JSON:
                    httpRequest.ContentType = "application/json";
                    break;
                case SubmitRequestType.X_WWW_FORM:
                    httpRequest.ContentType = "application/x-www-form-urlencoded";
                    break;
                case SubmitRequestType.OCTET_STREAM:
                    httpRequest.ContentType = "application/octet-stream";
                    break;
            }

            httpRequest.ContentLength = requestBytes.Length;

            try
            {
                using (Stream requestStream = httpRequest.GetRequestStream())
                {
                    requestStream.Write(requestBytes, 0, requestBytes.Length);
                    requestStream.Close();
                }
            }
            catch (WebException ex)
            {
                RequestData requestData = new RequestData(
                    httpRequest.RequestUri,
                    (RequestMethod)Enum.Parse(typeof(RequestMethod), httpRequest.Method, true),
                    new MemoryStream(requestBytes).ReadAll()
                );

                throw new ExtendedWebException(ex, new RequestResult(requestData, null), this);
            }
        }

        /// <summary>
        /// Builds HttpWebRequest object.
        /// </summary>
        /// <param name="absoluteUrl">Absolute URL</param>
        /// <param name="requestMethod">Request method (POST/GET/etc..)</param>
        protected virtual HttpWebRequest BuildWebRequest(Uri absoluteUrl, RequestMethod requestMethod)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(absoluteUrl);
            httpRequest.Timeout = Timeout;
            httpRequest.Method = requestMethod.ToString();

            httpRequest.CookieContainer = _sessionCookies;
            httpRequest.Proxy = null;

            httpRequest.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            return httpRequest;
        }

        #endregion

        #region Events

        public delegate void BeforeRequestEventHandler(SimpleWebRequest sender, RequestData request);
        public event BeforeRequestEventHandler BeforeRequest;

        private void OnBeforeRequestEvent(RequestData request)
        {
            BeforeRequest?.Invoke(this, request);
        }

        public delegate void AfterRequestEventHandler(SimpleWebRequest sender, RequestResult result);
        public event AfterRequestEventHandler AfterRequest;

        private void OnAfterRequestEvent(RequestResult result)
        {
            AfterRequest?.Invoke(this, result);
        }

        #endregion

        #region ToString()

        public override string ToString()
        {
            return Name ?? "N\\A";
        }

        #endregion
    }
}
