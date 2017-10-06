using PHP_Scripting.Install;
using PHP_Scripting.Ext;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace PHP_Scripting
{
    public class PhpScript
    {
        public string PhpFilePath { get; set; } = "";
        //public Dictionary<string, string> QueryString

        private PhpInstallation _Installation;
        
        public PhpScript(PhpInstallation install, string filePath)
        {
            this.PhpFilePath = filePath;
            _Installation = install;
        }

        public string Execute()
        {
            return ExecuteBinary(PhpFilePath);
        }
        public async Task<string> ExecuteAsync()
        {
            if (_Installation == null)
                throw new NullReferenceException("No PHP installation specified.");

            return await Task.Run(() => ExecuteBinary(PhpFilePath));
        }

        private string ExecuteBinary(string file)
        {
            StringBuilder ret = new StringBuilder();

            string filePath = Path.GetFullPath(file);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _Installation.PhpExecutablePath;
            psi.Arguments = $"-f \"{filePath}\"";
            //psi.EnvironmentVariables.Add("")
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            Process p = new Process();
            p.StartInfo = psi;
            
            p.Start();

            /*Task.Delay(_Installation.ExecutionTimeout).ContinueWith((t) =>
            {
                if (!p.HasExited)
                    p.Kill();
            });*/
            
            while (!p.StandardOutput.EndOfStream)
            {
                string line = p.StandardOutput.ReadLine();
                ret.AppendLine(line);
            }

            return ret.ToString();
        }
    }
}
