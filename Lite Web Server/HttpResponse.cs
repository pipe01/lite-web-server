using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string BodyContent { get; set; } = null;

        public string StatusName => GetStatusString();

        public HttpResponse() { }
        public HttpResponse(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }
        public HttpResponse(HttpStatusCode statusCode, string bodyContent) : this(statusCode)
        {
            this.BodyContent = bodyContent;
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("HTTP/1.0 ");
            ret.Append((int)StatusCode);
            ret.Append(GetStatusString() + "\r\n");

            foreach (var item in Headers)
            {
                ret.Append(item.Key + ": " + item.Value + "\r\n");
            }

            if (BodyContent != null)
            {
                ret.Append("\r\n");
                ret.Append(BodyContent);
            }
            
            return ret.ToString();
        }

        private string GetStatusString()
        {
            string enumName = Enum.GetName(typeof(HttpStatusCode), StatusCode);
            string ret = "" + enumName[0];

            for (int i = 1; i < enumName.Length; i++)
            {
                char c = enumName[i];
                char prevC = enumName[i - 1];

                if (char.IsUpper(c) && !char.IsUpper(prevC))
                    ret += " ";

                ret += char.ToLower(c);
            }

            return ret;
        }
    }
}
