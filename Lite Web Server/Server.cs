using Lite_Web_Server.File_Processors;
using PHP_Scripting.Install;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public class Server
    {
        private class NetworkState
        {
            public const int BufferSize = 1024;
            public byte[] Buffer = new byte[BufferSize];
            public TcpClient RemoteClient;
        }

        public int Port { get; set; } = 80;
        public string StaticFilesRoot { get; set; } = "./";
        public Dictionary<HttpStatusCode, string> ErrorPages { get; private set; } = new Dictionary<HttpStatusCode, string>();

        private TcpListener _Listener;
        private SemaphoreSlim _Semaphore = new SemaphoreSlim(0, 1);
        private List<TcpClient> _ReceivingClients = new List<TcpClient>();
        private ServerFiles _Files = new ServerFiles();
        private List<Thread> _Threads = new List<Thread>();
        private bool _Exit = false;
        private Configuration _Config;
        private PhpInstallation _PHP;
        private List<FileProcessor> _Processors;

        public Server(Configuration config)
        {
            _Config = config;
            _Processors = FileProcessor.GetAllProcessors(_Config).ToList();

            LoadConfig();
        }

        private void LoadConfig()
        {
            string errorFolder = _Config.Get("DefaultErrorFolder", "/error");

            _Files.FilesRoot = _Config.Get("StaticFilesPath", "./www");
            if (!Directory.Exists(_Files.FilesRoot))
                Directory.CreateDirectory(_Files.FilesRoot);

            if (errorFolder != null)
            {
                string serverPath = errorFolder + (errorFolder.EndsWith("/") ? "" : "/");

                if (!errorFolder.StartsWith("."))
                    errorFolder = "." + errorFolder;
                
                if (Directory.Exists(errorFolder))
                    foreach (var item in Directory.EnumerateFiles(errorFolder, "???.html", SearchOption.TopDirectoryOnly))
                    {
                        var filename = Path.GetFileNameWithoutExtension(item);

                        if (int.TryParse(filename, out int code))
                        {
                            ErrorPages.Add((HttpStatusCode)code, serverPath + filename + ".html");
                        }
                    }
            }

            foreach (var item in _Config)
            {
                var match = Regex.Match(item.Key, @"ErrorPage\d\d\d");

                if (match.Success)
                {
                    int code = int.Parse(item.Key.Substring(item.Key.Length - 3));
                    ErrorPages.Add((HttpStatusCode)code, item.Value as string);
                }
            }

            _PHP = new PhpInstallation(Program.GetPhpVersion(_Config));
        }

        public void Start()
        {
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);
            _Listener.Start();

            _Files.LoadFromDirectory();

            Console.WriteLine("Listening on port " + Port);

#pragma warning disable 4014
            AcceptConnections();
#pragma warning restore
        }

        public void Stop()
        {
            _Exit = true;

            foreach (var item in _ReceivingClients)
            {
                item.Close();
            }

            _Listener.Stop();

            _Semaphore.Release();
        }

        public async Task WaitForExit()
        {
            await _Semaphore.WaitAsync();
        }

        private async Task AcceptConnections()
        {
            while (!_Exit)
            {
                var conn = await _Listener.AcceptTcpClientAsync();

                var t = new Thread(() => StartReceive(conn));
                _Threads.Add(t);
                t.Start();
            }
        }

        private void StartReceive(TcpClient client)
        {
            NetworkState state = new NetworkState();
            state.RemoteClient = client;

            if (client.Client != null)
            {
                _ReceivingClients.Add(client);

                client.Client.BeginReceive(state.Buffer, 0, NetworkState.BufferSize, 0, ReceiveCallback, state);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (NetworkState)ar.AsyncState;
            var client = state.RemoteClient;

            _ReceivingClients.Remove(client);

            if (client.Client == null)
                return;

            int rec = client.Client.EndReceive(ar, out var error);

            if (error == SocketError.Success)
            {
                string str = Encoding.UTF8.GetString(state.Buffer).TrimEnd('\0');

                if (!string.IsNullOrWhiteSpace(str))
                {
                    HttpRequest req = RequestParser.Parse(str);
                    HttpResponse resp = ParseRequest(req);

                    SendResponse(client, req, resp);
                }
            }
            else
            {
                throw new Exception("Something went wrong. Error code: " + error);
            }
        }

        private HttpResponse ParseRequest(HttpRequest req)
        {
            HttpResponse response = new HttpResponse();
            var file = _Files.SearchFile(req.HostPath);

            try
            {
                
                if (file == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    bool errorPage = ErrorPages.ContainsKey(response.StatusCode);

                    //The error page is defined in the config, load it
                    if (errorPage)
                    {
                        file = _Files.SearchFile(ErrorPages[response.StatusCode]);
                    }

                    //Either the error page isn't defined, or the defined page can't be found
                    if (!errorPage || (errorPage && file == null))
                    {
                        response.Headers["Content-Type"] = "text/html";
                        response.BodyContent = $"<h1>Error {(int)response.StatusCode}: {response.StatusName}</h1>";
                    }
                }

                if (file != null)
                {
                    response.Headers["Content-Type"] = file.Value.MimeType;
                    response.BodyContent = _Files.ReadAllText(file.Value);
                }
            }
            catch (Exception)
            {
                response.Headers["Content-Type"] = "text/html";
                response.BodyContent = "<h1>Error 500: Internal server error</h1>";
            }
            finally
            {
                if (file != null)
                {
                    var processor = _Processors.FirstOrDefault(o => o.FileExtensions.Contains(file.Value.Extension));
                    if (processor != null)
                    {
                        var fileStr = processor.ProcessFile(file.Value);
                        response.BodyContent = fileStr;
                    }
                }
            }

            return response;
        }

        private void SendResponse(TcpClient client, HttpRequest request, HttpResponse response)
        {
            string logLine =
                DateTime.Now.ToString(@"\[yyyy\-MM\-dd hh\:mm\:ss\]") +
                $" {request.RemoteHost}: {request.Method} {request.HostPath} " +
                $"({(int)response.StatusCode} {response.StatusName})";
            Log.WriteLine(logLine);

            client.Client.Send(Encoding.UTF8.GetBytes(response.ToString()));
            client.Close();
        }
    }
}
