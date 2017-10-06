using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lite_Web_Server;
using System.IO;

namespace Lite_Web_Server_Test
{
    [TestClass]
    public class ServerFilesTest
    {
        private Configuration TestConfig;
        private ServerFiles Files;

        [TestInitialize]
        public void Initialize()
        {
            Directory.CreateDirectory("www");
            File.WriteAllText("www/index.html", "Index test contents");

            Directory.CreateDirectory("www/testFolder");
            File.WriteAllText("www/testFolder/testFile.txt", "Test text content");

            Files = new ServerFiles();
            Files.FilesRoot = "./www";
        }


        [TestCleanup]
        public void CleanUp()
        {
            Directory.Delete("www", true);
        }

        [TestMethod]
        public void SearchRootFileWithFileName()
        {
            Assert.IsNotNull(Files.SearchFile("index.html", "/"));
        }

        [TestMethod]
        public void SearchIndexFile()
        {
            Assert.IsNotNull(Files.SearchFile("/"));
        }

        [TestMethod]
        public void GetFileContents()
        {
            Assert.AreEqual("Test text content", Files.ReadAllText(Files.SearchFile("testFile.txt", "testFolder").Value));
        }
    }
}
