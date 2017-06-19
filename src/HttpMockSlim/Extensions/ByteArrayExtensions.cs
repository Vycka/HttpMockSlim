using System.IO;
using System.Text;

namespace HttpMockSlim.Extensions
{
    public static class ByteArrayExtensions
    {
        public static MemoryStream GenerateStream(this byte[] value)
        {
            return new MemoryStream(value);
        }

        /// <summary>
        /// Converts to UTF8 string
        /// </summary>
        public static string ConvertToString(this byte[] value)
        {
            return value.ConvertToString(Encoding.UTF8);
        }

        public static string ConvertToString(this byte[] value, Encoding encoding)
        {
            return encoding.GetString(value);
        }
    }
}