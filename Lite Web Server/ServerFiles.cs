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
        public string FileName;
        public string RelativePath;
        public string FileSystemPath;
        public bool IsFolder;
        
        public string FullServerPath => Path.Combine(RelativePath, FileName) + (IsFolder ? "/" : "");
        public string MimeType => Mime.GetMimeType(Path.GetExtension(FileName));
        public string Extension => Path.GetExtension(FileName);

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

        public string[] ValidIndexExtensions { get; set; } = { "html", "htm", "php" };
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
            LoadFromDirectory();

            if (!root.EndsWith("/"))
                root += "/";

            foreach (var item in _InnerList)
            {
                if (item.RelativePath.Equals(root, StringComparison.InvariantCultureIgnoreCase) &&
                    Regex.IsMatch(item.FileName, WildCardToRegular(filename)))
                {
                    return item;
                }
            }
            return new ServerFile?();
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
