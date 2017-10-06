using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lite_Web_Server;

namespace Lite_Web_Server_Test
{
    [TestClass]
    public class ServerTest
    {
        private Server Server;

        [TestInitialize]
        public void Initialize()
        {
            Configuration config = new Configuration(true);

            Server = new Server(config);
            Server.Start();
        }
    }
}
