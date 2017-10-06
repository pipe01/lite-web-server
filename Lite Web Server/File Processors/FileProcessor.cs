using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server.File_Processors
{
    /// <summary>
    /// Gets a file and tries to process it
    /// </summary>
    public abstract class FileProcessor
    {
        public FileProcessor(Configuration config) { }

        /// <summary>
        /// File extensions this processor accepts
        /// </summary>
        public abstract string[] FileExtensions { get; }

        /// <summary>
        /// Process a file and return its processed contents
        /// </summary>
        /// <param name="file">The file</param>
        /// <returns></returns>
        public abstract string ProcessFile(ServerFile file);

        /// <summary>
        /// Gets all the processors
        /// </summary>
        /// <param name="config">Configuration for creating the processors</param>
        /// <returns></returns>
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
