using System.IO;
using System.Net;
using System.Text;

namespace HttpMockSlim.Model
{
    public class Response
    {
        public int StatusCode = 200;
        public string ContentType = "text/plain";
        public Stream Body;
    }

    public static class ResponseExtensions
    {
        public static Response Code(this Response response, HttpStatusCode httpStatusCode)
        {

            return response;
        }

        public static Response Code(this Response response, int httpStatusCode)
        {
            return response;

        }

        public static Response Body(this Response response, string text)
        {
            response.Body = GenerateStreamFromString(text);

            return response;
            
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }
    }
}