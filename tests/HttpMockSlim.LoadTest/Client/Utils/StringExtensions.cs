using System;

namespace HttpMockSlim.LoadTest.Client.Utils
{
    public static class StringExtensions
    {
        public static string SmartTrim(this string source, int maxLength)
        {
            string result;

            if (!String.IsNullOrWhiteSpace(source))
            {

                bool trimData = source.Length > maxLength;

                result = trimData ? source.Substring(0, maxLength) : source;
                if (trimData)
                    result += "...";
            }
            else
            {
                result = "";
            }

            return result;
        }
    }
}