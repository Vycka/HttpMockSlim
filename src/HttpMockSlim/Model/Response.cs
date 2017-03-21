using System.IO;
using System.Net;
using System.Text;
using HttpMockSlim.Extensions;

namespace HttpMockSlim.Model
{
    public class Response
    {
        public int StatusCode = 200;
        public string ContentType = "text/plain";
        public Stream Body;

        /// <summary>
        /// Sets body with UTF8 encoding.
        /// </summary>
        public void SetBody(string value)
        {
            Body = value.GenerateStream();
        }

        public void SetBody(byte[] value)
        {
            Body = value.GenerateStream();
        }
    }
}