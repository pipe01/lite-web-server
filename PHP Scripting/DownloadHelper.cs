using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PHP_Scripting
{
    public class DownloadHelper
    {
        public class DownloadProgressEventArgs : EventArgs
        {
            public float DownloadPercentage { get; internal set; } = 0;
            public TimeSpan TimeRemaining { get; internal set; } = TimeSpan.FromSeconds(0);
        }

        public delegate void DownloadProgressEventHandler(object sender, DownloadProgressEventArgs e);
        public event DownloadProgressEventHandler DownloadProgressChanged = delegate { };

        public string URL { get; private set; } = "";
        public string FileName { get; private set; } = "";

        public DownloadHelper() { }
        public DownloadHelper(string url, string filename = null)
        {
            this.URL = url;
            this.FileName = filename ?? ("./" + System.IO.Path.GetFileName(url));
        }

        public void Download()
        {
            DownloadAsync().GetAwaiter().GetResult();
        }
        public async Task DownloadAsync()
        {
            WebClient client = new WebClient();

            DateTime started = DateTime.Now;

            client.DownloadProgressChanged += (a, b) =>
            {
                lock (client)
                {
                    TimeSpan elapsed = DateTime.Now - started;
                    TimeSpan estimated = TimeSpan.FromSeconds(
                        (b.TotalBytesToReceive - b.BytesReceived) /
                        (b.BytesReceived / elapsed.TotalSeconds));

                    var args = new DownloadProgressEventArgs()
                    {
                        DownloadPercentage = ((float)b.BytesReceived / b.TotalBytesToReceive) * 100,
                        TimeRemaining = estimated
                    };

                    DownloadProgressChanged(null, args);
                }
            };

            await client.DownloadFileTaskAsync(URL, FileName);
        }
    }
}
