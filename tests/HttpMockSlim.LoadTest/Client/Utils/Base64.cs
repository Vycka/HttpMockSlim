using System;
using System.Text;

namespace HttpMockSlim.LoadTest.Client.Utils
{
    public static class Base64
    {
        public static string Decode(string input)
        {
            int mod4 = input.Length % 4;
            if (mod4 > 0)
            {
                input += new string('=', 4 - mod4);
            }

            string result = Encoding.UTF8.GetString(Convert.FromBase64String(input));

            return result;
        }
    }
}