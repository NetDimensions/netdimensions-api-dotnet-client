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
            string s = "module".Equals(call.FunctionName)
                ? ExcellenceTests.Properties.Resources.module
                : ExcellenceTests.Properties.Resources.learningPath;
            return call.ResponseParser(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s)));
        }
    }

    [TestClass]
    public class ExcellenceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            item unenrolledProgram = new Excellence(new MockClient(), "netd_rob")
                .GetExpandedLearningPath()
                .jobProfile[2]
                .competency[0]
                .sequence[0]
                .item[0];
            Assert.AreEqual(2, unenrolledProgram.sequence.Length);
        }
    }
}
