using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server.File_Processors
{
    public abstract class FileProcessor
    {
        public FileProcessor(Configuration config) { }
        public abstract string[] FileExtensions { get; }

        public abstract string ProcessFile(ServerFile file);

        public static IEnumerable<FileProcessor> GetAllProcessors(Configuration config)
        {
            foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (item.Namespace == "Lite_Web_Server.File_Processors" &&
                    !item.IsAbstract && item.BaseType == typeof(FileProcessor))
                {
                    yield return Activator.CreateInstance(item, config) as FileProcessor;
                }
            }
        }
    }
}
