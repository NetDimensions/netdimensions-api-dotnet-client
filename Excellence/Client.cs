using NetDimensions.Apis.LearningPath;
using NetDimensions.Apis.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetDimensions.Apis
{
    class Parameter
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public Parameter(string paramName, string paramValue)
        {
            Name = paramName;
            Value = paramValue;
        }

        public override string ToString()
        {
            return Uri.EscapeDataString(Name) + "=" + Uri.EscapeDataString(Value);
        }

        public static string ToString(IEnumerable<Parameter> parameters)
        {
            return string.Join("&", parameters);
        }
    }

    delegate T Parser<T>(System.IO.Stream str);

    class Client
    {
        private readonly string baseUrl;
        private readonly NetworkCredential credentials;
        private readonly string onBehalfOf;
        public Client(string url, NetworkCredential cred, string onBehalfOf)
        {
            this.baseUrl = url;
            this.credentials = cred;
            this.onBehalfOf = onBehalfOf;
        }

        private T Get<T>(string functionName, IEnumerable<Parameter> parameters, Parser<T> responseParser)
        {
            WebRequest req = WebRequest.Create(baseUrl + "api/" + functionName + "?"
                + Parameter.ToString(parameters.Concat(new[] { new Parameter("onBehalfOf", onBehalfOf) })));
            req.Credentials = credentials;
            using (WebResponse resp = req.GetResponse())
            {
                return responseParser(resp.GetResponseStream());
            }
        }

        public trainingHistory getRecords()
        {
            return Get("records",
                new Parameter[] { },
                stream => (trainingHistory)new XmlSerializer(typeof(trainingHistory)).Deserialize(stream));
        }

        public learningPath getLearningPath(string assignmentId)
        {
            return Get("learningPath",
                new[] { new Parameter("format", "xml"), new Parameter("assignmentId", assignmentId) },
                stream => (learningPath)new XmlSerializer(typeof(learningPath)).Deserialize(stream));
        }

        public NetDimensions.Apis.Module.module getModule(string id, string assignmentId)
        {
            return Get("module",
                new[] { new Parameter("id", id), new Parameter("assignmentId", assignmentId) },
                stream => (NetDimensions.Apis.Module.module)new XmlSerializer(typeof(NetDimensions.Apis.Module.module)).Deserialize(stream));
        }
    }
}
