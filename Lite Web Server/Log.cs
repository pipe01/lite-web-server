using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public static class Log
    {
        public static string LogFilePath { get; set; } = "log.log";

        public static void WriteLine(string format, params object[] args)
        {
            string line = string.Format(format, args: args);
            Console.WriteLine(line);
            File.AppendAllLines(LogFilePath, new[] { line });
        }
    }
}
