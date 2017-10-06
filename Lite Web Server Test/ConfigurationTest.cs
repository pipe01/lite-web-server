using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lite_Web_Server;

namespace Lite_Web_Server_Test
{
    [TestClass]
    public class ConfigurationTest
    {
        private bool SetGeneric<T>(string key, T value)
        {
            Configuration cfg = new Configuration(true);

            cfg.Set(key, value);

            return value.Equals(cfg.Get(key, default(T)));
        }

        [TestMethod]
        public void SetString()
        {
            Assert.IsTrue(SetGeneric("TestString", "This is a test"));
        }

        [TestMethod]
        public void SetInt()
        {
            Assert.IsTrue(SetGeneric("TestInt", 42));
        }
    }
}
