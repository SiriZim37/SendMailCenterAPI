

namespace SendMailAPI.Class
{
    public class GetConnection
    {
        private static GetConnection c_connect;
        public string _connectStr;
        public string _connectStrEx;
        private GetConnection() { }
        public static GetConnection GetConnectionStr()
        {
            if (c_connect == null)
            {
                c_connect = new GetConnection();
            }
            return c_connect;
        }
    }
}
