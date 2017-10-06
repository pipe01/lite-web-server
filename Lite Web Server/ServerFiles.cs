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
        /// File name and extension
        /// </summary>
        public string FileName;

        /// <summary>
        /// Path relative to server root, not including file name
        /// </summary>
        public string RelativePath;

        /// <summary>
        /// Path relative to computer file system, including file name
        /// </summary>
        public string FileSystemPath;

        /// <summary>
        /// Whether this file is a folder
        /// </summary>
        public bool IsFolder;
        
        public string FullServerPath => Path.Combine(RelativePath, FileName) + (IsFolder ? "/" : "");
        public string MimeType => Mime.GetMimeType(Path.GetExtension(FileName));
        public string Extension => Path.GetExtension(FileName);

        /// <summary>
        /// Create new ServerFile
        /// </summary>
        /// <param name="fileName">File name with extension</param>
        /// <param name="relativePath">File path relative to server root without extension</param>
        /// <param name="isFolder">Is this a folder?</param>
        /// <param name="FileSystemPath">Full computer file system path</param>
        public ServerFile(string fileName, string relativePath, bool isFolder, string FileSystemPath)
        {
            this.FileName = fileName;
            this.RelativePath = relativePath;
            this.IsFolder = isFolder;
            this.FileSystemPath = FileSystemPath;
        }
    }

    public class ServerFiles : IReadOnlyList<ServerFile>
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
        
        public ServerFile this[int index] => _InnerList[index];
        public int Count => _InnerList.Count;
        public IEnumerator<ServerFile> GetEnumerator() => _InnerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _InnerList.GetEnumerator();
        
        public void LoadFromDirectory()
        {
            string root = Path.GetFullPath(FilesRoot);

            _InnerList.Clear();

            //Get all files
            foreach (var item in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(item);
                string relative = ToRelativePath(item, root);
                string fileFolder = Path.GetDirectoryName(relative)?.Replace('\\', '/');

                fileFolder = fileFolder ?? "/";

                if (fileFolder.Last() != '/')
                    fileFolder += "/";

                _InnerList.Add(new ServerFile(fileName, fileFolder, false, item));
            }

            //Get all folders
            foreach (var item in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(item);
                string fileFolder = Path.GetDirectoryName(ToRelativePath(item, root))?.Replace('\\', '/');

                if (fileFolder == null)
                    continue;

                _InnerList.Add(new ServerFile(fileName, fileFolder, true, item));
            }
        }

        public string ReadAllText(ServerFile file)
        {
            return File.ReadAllText(GetLocalFilePath(file));
        }

        public string GetLocalFilePath(ServerFile file)
        {
            return Path.Combine(Path.GetFullPath(FilesRoot), file.RelativePath.TrimStart('/'), file.FileName);
        }

        public ServerFile? SearchFile(string fullPath)
        {
            string fileName = Path.GetFileName(fullPath);
            string filePath = Path.GetDirectoryName(fullPath)?.Replace('\\', '/');
            filePath = filePath ?? "/";

            bool isFolder = _InnerList.Any(o => o.IsFolder && o.FileName == fileName && o.RelativePath == filePath);

            if (isFolder)
            {
                filePath += fileName + "/";
                fileName = "";
            }

            if (fileName == "") //Search for index.html etc
            {
                foreach (var item in ValidIndexExtensions)
                {
                    var file = SearchFile("index." + item, filePath);

                    if (file != null)
                        return file;
                }
                return null;
            }
            else
            {
                return SearchFile(fileName, filePath);
            }
        }
        public ServerFile? SearchFile(string filename, string root)
        {
            //The root is just "/", so make it empty
            if (root == "/")
                root = "";

            //Remove "/" from the start of the root
            root = root.TrimStart('/', '\\');

            //Get the full path of the server's file root
            var fullFileRoot = Path.GetFullPath(FilesRoot);

            //Combine the relative file path and the full file root
            var combinedPath = Path.Combine(fullFileRoot, root, filename);

            //Check if it exists
            var fileExists = File.Exists(combinedPath);
            
            //If it exists, return it
            if (fileExists)
            {
                return new ServerFile(filename, root + filename, false, combinedPath);
            }

            return null;
        }

        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private static string ToRelativePath(string path, string rootPath)
        {
            return path.Replace(rootPath, "").Replace('\\', '/');
        }
    }
}
