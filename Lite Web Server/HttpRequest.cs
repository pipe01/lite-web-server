using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public struct HttpRequest
    {
        public string Method;
        public string HostPath;
        public Dictionary<string, string> Headers;
        public string RemoteHost;

        public HttpRequest(string method, string hostPath, string remoteHost, Dictionary<string, string> headers)
        {
            Method = method;
            HostPath = hostPath;
            RemoteHost = remoteHost;
            Headers = headers;
        }
    }
}
