using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHP_Scripting.Install
{
    public class PhpInstallation
    {
        public PhpVersion Version { get; internal set; }
        public string PhpExecutablePath { get; internal set; }
        public int ExecutionTimeout { get; set; } = 2000;

        public PhpInstallation() { }
        public PhpInstallation(string customPath)
        {
            this.Version = new PhpVersion();

            this.PhpExecutablePath = Path.Combine(customPath, "php-cgi.exe");

            if (!File.Exists(this.PhpExecutablePath))
                throw new ArgumentException("The PHP executable file was not found.", nameof(customPath));
        }
        public PhpInstallation(PhpVersion version)
        {
            this.Version = version;

            string executablePath = "./php/" + version.VersionNumber + "/php-cgi.exe";
            executablePath = Path.GetFullPath(executablePath);

            if (File.Exists(executablePath))
                this.PhpExecutablePath = executablePath;
            else
                throw new ArgumentException("The PHP executable file was not found.", nameof(executablePath));
        }
    }
}
