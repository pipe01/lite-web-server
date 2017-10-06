using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PHP_Scripting.Install
{
    public static class Installer
    {
        private static List<PhpVersion> _VersionCache;

        public static event DownloadHelper.DownloadProgressEventHandler DownloadProgressChanged = delegate { };

        /// <summary>
        /// Returns a copy of the available versions cache
        /// </summary>
        public static List<PhpVersion> AvailableVersions
            => new List<PhpVersion>(_VersionCache ?? (_VersionCache = VersionSniffer.GetAvailableVersions().ToList()));

        /// <summary>
        /// Downloads a PHP version synchronously
        /// </summary>
        /// <param name="version">The PHP version returned by AvailableVersions</param>
        public static void DownloadVersion(Version version)
        {
            DownloadVersionAsync(version).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Downloads a PHP version asynchronously
        /// </summary>
        /// <param name="version"></param>
        public static async Task DownloadVersionAsync(Version version)
        {
            var query = _VersionCache.Where(o => o.VersionNumber == version);

            if (!query.Any())
                throw new ArgumentException("The specified PHP version was not found", nameof(version));

            PhpVersion ver = query.Single();

            if (File.Exists("php.zip"))
                File.Delete("php.zip");

            var downloader = new DownloadHelper(ver.DownloadURL, "php.zip");
            downloader.DownloadProgressChanged += (a, b) => DownloadProgressChanged(a, b);

            await downloader.DownloadAsync();

            await ExtractPackage(ver.VersionNumber);
        }

        private static async Task ExtractPackage(Version version)
        {
            string extractPath = "php/" + version;

            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            try
            {
                using (ZipFile zip = ZipFile.Read("php.zip"))
                {
                    await Task.Run(() => zip.ExtractAll(extractPath));
                }
            }
            finally
            {
                File.Delete("php.zip");
            }
        }
    }
}
