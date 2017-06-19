using System.IO;
using System.Text;

namespace HttpMockSlim.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (MemoryStream reader = new MemoryStream())
            {
                stream.CopyTo(reader);

                return reader.ToArray();
            }
        }

        public static string ReadAll(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}