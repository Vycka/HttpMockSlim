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
    }
}