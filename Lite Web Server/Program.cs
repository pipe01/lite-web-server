using CliWrap;
using PHP_Scripting;
using PHP_Scripting.Install;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public static class Program
    {
        private static List<PhpVersion> PhpVersions;

        static void Main(string[] args)
        {
            Configuration config = new Configuration();
            config.Load();
            
            PhpVersions = Installer.AvailableVersions;
            PhpVersions.Sort((a, b) => a.VersionNumber.CompareTo(b.VersionNumber));

            var phpVersion = GetPhpVersion(config);

            if (!config.TryGet("PHPCheckVCRedist", out bool check) || check)
            {
                CheckVCRedist(phpVersion);
            }

            Server s = new Server(config);
            
            Console.CancelKeyPress += (a, b) =>
            {
                b.Cancel = true;
                s.Stop();
            };
            
            s.Start();
            s.WaitForExit().GetAwaiter().GetResult();
        }

        private static void InstallVCRedist(VcRedistChecker.VcRedistributable vc)
        {
            if (!File.Exists("vc.exe"))
                return;

            Console.Clear();
            Console.WriteLine("Installing...");

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo
            {
                FileName = "./vc.exe",
                Arguments = vc.InstallArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            p.Start();

            string progressChars = @"-\|/";
            int progress = 0;
            while (!p.HasExited)
            {
                Console.SetCursorPosition(14, 0);
                Console.Write(progressChars[progress]);

                if (++progress == progressChars.Length)
                    progress = 0;

                Thread.Sleep(200);
            }

            Console.Clear();
            Console.WriteLine("Installed!");
        }

        private static void CheckVCRedist(PhpVersion phpVersion)
        {
            if (!phpVersion.VisualCRedist.Installed)
            {
                Console.Write("Visual C++ Redistributable {0} not installed. Install now? (Y/n): ", phpVersion.VisualCRedist.Name);
                var input = Console.ReadLine();

                if (input == "" || input.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Downloading...");

                    var downloader = new DownloadHelper(phpVersion.VisualCRedist.DownloadURL, "vc.exe");

                    downloader.DownloadProgressChanged += (a, b) =>
                    {
                        Console.SetCursorPosition(0, 0);
                        Console.Write("{0:0}%\t{1:0} s   ", b.DownloadPercentage, b.TimeRemaining.TotalSeconds);
                    };

                    Console.Clear();
                    downloader.Download();

                    Console.Clear();
                    Console.WriteLine("Download complete!");
                    Thread.Sleep(500);

                    InstallVCRedist(phpVersion.VisualCRedist);

                    File.Delete("vc.exe");
                }
            }
        }

        public static PhpVersion GetPhpVersion(Configuration config)
        {
            string phpVersionStr = config.Get("PhpVersion", "latest");
            PhpVersion phpVersion = default(PhpVersion);

            if (phpVersionStr.Equals("latest", StringComparison.InvariantCultureIgnoreCase))
            {
                phpVersion = PhpVersions.Last();
            }
            else if (Version.TryParse(phpVersionStr, out var ver))
            {
                phpVersion = PhpVersions.Single(o => o.VersionNumber == ver);
            }

            if (!phpVersion.Installed)
            {
                Console.WriteLine("Downloading PHP version " + phpVersion);

                Installer.DownloadProgressChanged += (a, b) =>
                {
                    Console.SetCursorPosition(0, 0);
                    Console.Write("{0}%\t{1} seconds remaining   ",
                        b.DownloadPercentage.ToString("0"),
                        b.TimeRemaining.TotalSeconds.ToString("0.0"));
                };

                Installer.DownloadVersion(phpVersion.VersionNumber);
            }

            Console.Clear();
            return phpVersion;
        }
    }
}
