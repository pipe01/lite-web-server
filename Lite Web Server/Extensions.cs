using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public static class Extensions
    {
        public static async Task<TcpClient> AcceptTcpClientTimeout(this TcpListener list, int timeout, int resolution = 10)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                if (sw.ElapsedMilliseconds > timeout)
                    return null;

                if (list.Pending())
                    return await list.AcceptTcpClientAsync();

                await Task.Delay(resolution);
            }
        }
    }
}
