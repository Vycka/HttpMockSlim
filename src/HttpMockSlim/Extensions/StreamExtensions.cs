using System.IO;
using System.IO.Compression;

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
        /// Decompresses a GZip stream
        /// </summary>
        /// <param name="stream">GZip compressed stream</param>
        /// <returns>Decompressed stream</returns>
        public static Stream DecompressGZip(this Stream stream)
        {
            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                MemoryStream result = new MemoryStream();

                gZipStream.CopyTo(result);

                result.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }
    }
}