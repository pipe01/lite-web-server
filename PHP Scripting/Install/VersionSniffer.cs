using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PHP_Scripting.Install
{
    public struct PhpVersion
    {
        public Version VersionNumber;
        public string DownloadURL;
        public VcRedistChecker.VcRedistributable VisualCRedist;
        public bool Installed;

        public PhpVersion(Version versionNumber, string downloadURL, bool installed, VcRedistChecker.VcRedistributable visualCRedist)
        {
            this.VersionNumber = versionNumber;
            this.DownloadURL = downloadURL;
            this.Installed = installed;
            this.VisualCRedist = visualCRedist;
        }
    }

    internal static class VersionSniffer
    {
        
        public static string[] PhpVersionEndpoints { get; set; } =
        {
            "http://windows.php.net/downloads/releases/archives/",
            "http://windows.php.net/downloads/releases/"
        };

        public static IEnumerable<PhpVersion> GetAvailableVersions()
        {
            //TODO Maybe I should make this local... meh

            HtmlDocument doc = new HtmlDocument();
            var brick = new HtmlWebException("Errors while parsing versions.");
            
            foreach (var versionEndpoint in PhpVersionEndpoints)
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(versionEndpoint))
                    doc.Load(stream);

                if ((doc.ParseErrors != null && doc.ParseErrors.Count() > 0) || doc.DocumentNode == null)
                {
                    throw brick;
                }

                var pre = doc.DocumentNode.SelectSingleNode("//body/pre");

                if (pre == null)
                    throw brick;

                foreach (var item in pre.SelectNodes("//a"))
                {
                    var href = item.GetAttributeValue("href", null);

                    if (href == null)
                        throw brick;
                    //^(?!.*-nts-).*(?<=-Win32-)(VC.*)(?=-x86\.zip)
                    var regex = Regex.Match(href, @"(?<=php-)((\d+)\.(\d+)\.(\d+))(?=-Win32-VC\d+-x86\.zip)");
                    var vcRegex = Regex.Match(href, @"^(?!.*-nts-).*(?<=-Win32-)(VC.*)(?=-x86\.zip)");

                    if (regex.Success && vcRegex.Success && Version.TryParse(regex.Value, out var ver))
                    {
                        string filePath = "php/" + ver + "/php.exe";
                        string vc = vcRegex.Groups[1].Value.Substring(2);

                        if (!int.TryParse(vc, out int vcVersion))
                            continue;

                        bool installed = System.IO.File.Exists(filePath);

                        yield return new PhpVersion(ver, "http://windows.php.net" + href,
                            installed, VcRedistChecker.GetVC(vcVersion));
                    }
                }
            }
        }
    }
}
