using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public static class RequestParser
    {
        public static string NewLineSeparator { get; set; } = "\r\n";

        public static HttpRequest Parse(string str)
        {
            string[] lines = str.Split(new[] { NewLineSeparator }, StringSplitOptions.RemoveEmptyEntries);

            string _Method = "";
            string _HostPath = "";
            string _RemoteHost = "";
            Dictionary<string, string> _Headers = new Dictionary<string, string>();

            foreach (var item in lines)
            {
                if (item.StartsWith("GET") && _Method == "")
                {
                    _Method = "GET";

                    string[] split = item.Split(' ');
                    _HostPath = split[1];

                    continue;
                }

                int separatorIndex = item.IndexOf(':');
                string headerName = item.Substring(0, separatorIndex);
                string headerValue = item.Substring(separatorIndex + 2);

                if (headerName == "Host")
                {
                    _RemoteHost = headerValue;
                    continue;
                }

                _Headers.Add(headerName, headerValue);
            }

            return new HttpRequest(_Method, _HostPath, _RemoteHost, _Headers);
        }
    }
}
