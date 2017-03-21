using System.IO;
using System.Text;

namespace HttpMockSlim.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Generates stream with UTF8 encoding
        /// </summary>
        public static MemoryStream GenerateStream(this string value)
        {
            return GenerateStream(value, Encoding.UTF8);
        }

        public static MemoryStream GenerateStream(this string value, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(value ?? ""));
        }
    }
}