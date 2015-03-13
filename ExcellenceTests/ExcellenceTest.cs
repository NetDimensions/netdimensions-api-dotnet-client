using ExcellenceTests.Properties;
using NetDimensions.Apis;
using NetDimensions.Apis.LearningPath;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetDimensions.Excellence
{
    internal class MockClient : Client
    {
        private readonly string learningPathResponse;
        private readonly string moduleResponse;
        internal MockClient(string lpResp, string moduleResp)
        {
            this.learningPathResponse = lpResp;
            this.moduleResponse = moduleResp;
        }
        protected override T Get<T>(Call<T> call)
        {
            string s = "module".Equals(call.FunctionName) ? moduleResponse : learningPathResponse;
            return call.ResponseParser(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s)));
        }
    }

    [TestClass]
    public class ExcellenceTest
    {
        [TestMethod]
        public void TestModulesAddedForUnenrolledProgram()
        {
            item unenrolledProgram = UnenrolledProgram();
            Assert.AreEqual(2, unenrolledProgram.sequence.Length);
        }

        private static item UnenrolledProgram()
        {
            return Excellence()
                .GetExpandedLearningPath()
                .jobProfile[2]
                .competency[0]
                .sequence[0]
                .item[0];
        }

        private static Excellence Excellence()
        {
            return new Excellence(new MockClient(Resources.learningPath, Resources.module), "netd_rob");
        }

        [TestMethod]
        public void TestTypeCodeForAddedModule()
        {
            item unenrolledProgram = UnenrolledProgram();
            Assert.AreEqual(typeCode.onlineModule, unenrolledProgram.sequence[0].item[0].module.type.code);
        }

        [TestMethod]
        public void TestUrlForAddedModule()
        {
            item unenrolledProgram = UnenrolledProgram();
            Assert.AreEqual("https://preview.netdimensions.com/preview/servlet/ekp?CID=230979_eng&TX=FORMAT1",
                unenrolledProgram.sequence[0].item[0].url);
        }

        [TestMethod]
        public void TestGetUnitRefreshNone()
        {
            var profiles = new Excellence(new MockClient(Resources.LearningPathTechnicianLevel2,
                Resources.ModuleRevision0), "user1").GetUnitRefresh();
            Assert.AreEqual(1, profiles.Count());
            var competencies = profiles.First().Competencies;
            Assert.AreEqual(1, competencies.Count());
            Assert.IsFalse(competencies.First().Modules.Any());
        }

        [TestMethod]
        public void TestGetUnitRefreshOne()
        {
            var profiles = new Excellence(new MockClient(Resources.LearningPathTechnicianLevel2,
                Resources.ModuleRevision1), "user1").GetUnitRefresh();
            Assert.AreEqual(1, profiles.Count());
            var competencies = profiles.First().Competencies;
            Assert.AreEqual(1, competencies.Count());
            Assert.IsTrue(competencies.First().Modules.Any());
        }
    }
}
