using NetDimensions.Apis.LearningPath;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcellenceTests
{
    [TestClass]
    public class ExcellenceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string xml = ExcellenceTests.Properties.Resources.learningPath;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            Stream stream = new MemoryStream(bytes);
            learningPath lp = (learningPath) new XmlSerializer(typeof(learningPath)).Deserialize(stream);
            Assert.AreEqual(3, lp.jobProfile.Length);
        }
    }
}
