using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lite_Web_Server
{
    public struct ServerFile
    {
        /// <summary>
        /// File path relative to server root
        /// </summary>
        public string FilePath;

        /// <summary>
        /// File path relative to PC
        /// </summary>
        public string AbsoluteFilePath;

        public string MimeType => Mime.GetMimeType(this.Extension);
        public string Extension => Path.GetExtension(FilePath);

        /// <summary>
        /// Create new ServerFile
        /// </summary>
        /// <param name="fileName">File name with extension</param>
        /// <param name="relativePath">File path relative to server root without extension</param>
        /// <param name="isFolder">Is this a folder?</param>
        /// <param name="FileSystemPath">Full computer file system path</param>
        public ServerFile(string serverFilePath, string pcFilePath)
        {
            this.FilePath = serverFilePath;
            this.AbsoluteFilePath = pcFilePath;
        }
    }

    public class ServerFiles
    {
        private List<ServerFile> _InnerList = new List<ServerFile>();

        /// <summary>
        /// List of valid extensions for default index page
        /// </summary>
        public string[] ValidIndexExtensions { get; set; } = { "html", "htm", "php" };

        /// <summary>
        /// Server content root
        /// </summary>
        public string FilesRoot { get; set; } = "./";
        
        public string ReadAllText(ServerFile file)
        {
            return File.ReadAllText(GetFullPathForServerFile(file.FilePath));
        }
        
        public ServerFile? SearchFile(string fullPath)
        {
            //Get the path file name
            string fileName = Path.GetFileName(fullPath);

            //Try to get the directory path for the file path
            string filePath = Path.GetDirectoryName(fullPath)?.Replace('\\', '/');

            //If it's null (the file is on the root), make it a "/"
            filePath = filePath ?? "/";

            //Check if it's a folder
            bool isFolder = Directory.Exists(GetFullPathForServerFile(fullPath));

            if (isFolder)
            {
                //If it's a folder, append a "/" to the file path and make the file name empty
                filePath += fileName + "/";
                fileName = "";
            }

            //It's a folder, the file name is empty
            if (fileName == "")
            {
                //Search for valid index files
                foreach (var item in ValidIndexExtensions)
                {
                    var file = SearchFile("index." + item, filePath);

                    if (file != null)
                        return file;
                }
                return null;
            }
            else //It's a file, search it
            {
                return SearchFile(fileName, filePath);
            }
        }
        public ServerFile? SearchFile(string filename, string root)
        {
            //The root is just "/", so make it empty
            if (root == "/")
                root = "";

            //Get file system path for server file
            var filePath = GetFullPathForServerFile(root + "/" + filename);

            //Check if it exists
            var fileExists = File.Exists(filePath);
            
            //If it exists, return it
            if (fileExists)
            {
                return new ServerFile(Path.Combine(root, filename), filePath);
            }

            return null;
        }

        private string GetFullPathForServerFile(string serverFilePath)
        {
            return Path.Combine(Path.GetFullPath(FilesRoot), serverFilePath.TrimStart('/', '\\'));
        }
    }
}
