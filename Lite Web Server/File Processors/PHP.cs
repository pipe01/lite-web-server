using PHP_Scripting;
using PHP_Scripting.Install;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server.File_Processors
{
    public class PHP : FileProcessor
    {
        private static PhpInstallation _Installation;

        public PHP(Configuration config) : base(config)
        {
            if (_Installation == null)
            {
                if (config.TryGet("CustomPhpPath", out string path))
                {
                    _Installation = new PhpInstallation(path);
                }
                else
                {
                    _Installation = new PhpInstallation(Program.GetPhpVersion(config));
                }
                _Installation.ExecutionTimeout = config.Get("PhpExecutionTimeout", 4);
            }
        }

        public override string[] FileExtensions => new[] { ".php" };

        public override string ProcessFile(ServerFile file)
        {
            return new PhpScript(_Installation, file.FileSystemPath).Execute();
        }
    }
}
