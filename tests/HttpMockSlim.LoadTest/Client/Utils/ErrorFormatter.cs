using System.Linq;
using System.Text;

namespace  HttpMockSlim.LoadTest.Client.Utils
{
    public static class ErrorFormatter
    {
        public static string FormatError(params object[] objects)
        {
            var errorStringBuilder = new StringBuilder(1024);
            errorStringBuilder.AppendLine("#######################################################");
            errorStringBuilder.AppendLine("######################## BEGIN ########################");

            foreach (object source in objects.Where(o => o != null))
            {
                errorStringBuilder.AppendLine(source.ToString());
                errorStringBuilder.AppendLine("#######################################################");
            }

            errorStringBuilder.AppendLine("######################### EOF #########################");

            return errorStringBuilder.ToString();
        }
    }
}