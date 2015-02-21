using NetDimensions.Apis;
using NetDimensions.Apis.LearningPath;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetDimensions.Excellence
{
    internal class MockClient : Client
    {
        protected override T Get<T>(Call<T> call)
        {
            string s = ExcellenceTests.Properties.Resources.learningPath;
            return call.ResponseParser(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s)));
        }
    }

    [TestClass]
    public class ExcellenceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(3, new MockClient().GetLearningPath("netd_rob").jobProfile.Length);
        }
    }
}
