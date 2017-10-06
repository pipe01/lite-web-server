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
            //We still haven't created a PHP installation
            if (_Installation == null)
            {
                //If we have a custom PHP path in the config, use it
                if (config.TryGet("CustomPhpPath", out string path))
                {
                    _Installation = new PhpInstallation(path);
                }
                else //Else, create an installation with the version in the config
                {
                    _Installation = new PhpInstallation(Program.GetPhpVersion(config));
                }

                //Set the execution timeout from the config
                _Installation.ExecutionTimeout = config.Get("PhpExecutionTimeout", 4);
            }
        }

        public override string[] FileExtensions => new[] { ".php" };

        public override string ProcessFile(ServerFile file)
        {
            return new PhpScript(_Installation, file.AbsoluteFilePath).Execute();
        }
    }
}
