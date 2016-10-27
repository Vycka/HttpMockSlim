namespace HttpMockSlim.Model
{
    public class Request
    {
        public string Method;
        public string RawUrl;
        public string Body;

        public override string ToString()
        {
            return $"{Method}\t\t{RawUrl}\r\n\r\n{Body}";
        }
    }
}