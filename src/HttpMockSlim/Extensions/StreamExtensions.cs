using System.IO;
using System.IO.Compression;
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

        /// <summary>
        /// Decompress a GZip stream and return the content as a string
        /// </summary>
        /// <param name="stream">Compressed stream</param>
        /// <returns>Content of the compressed stream</returns>
        public static string DecompressStream(this Stream stream)
        {
            var buffer = new byte[1024];

            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                gZipStream.Read(buffer, 0, buffer.Length);
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}