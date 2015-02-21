using NetDimensions.Apis.CompetenciesAwarded;
using NetDimensions.Apis.LearningPath;
using NetDimensions.Apis.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

namespace NetDimensions.Apis
{
    public class Parameter
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

    public delegate T Parser<T>(System.IO.Stream str);

    public class Call<T>
    {
        public string FunctionName { get; private set; }
        public IEnumerable<Parameter> Parameters { get; private set; }
        public Parser<T> ResponseParser { get; private set; }
        internal Call(string functionName, IEnumerable<Parameter> parameters, Parser<T> responseParser)
        {
            FunctionName = functionName;
            Parameters = parameters;
            ResponseParser = responseParser;
        }
    }

    public abstract class Client
    {
        protected abstract T Get<T>(Call<T> call);

        internal static Call<T> Call<T>(string functionName, IEnumerable<Parameter> parameters, Parser<T> responseParser)
        {
            return new Call<T>(functionName, parameters, responseParser);
        }

        public trainingHistory GetRecords()
        {
            return Get(Call("records",
                new Parameter[] { },
                stream => (trainingHistory)new XmlSerializer(typeof(trainingHistory)).Deserialize(stream)));
        }

        public learningPath GetLearningPath(string assignmentId)
        {
            return Get(Call("learningPath",
                new[] { new Parameter("format", "xml"), new Parameter("assignmentId", assignmentId) },
                stream => (learningPath)new XmlSerializer(typeof(learningPath)).Deserialize(stream)));
        }

        public NetDimensions.Apis.Module.module GetModule(string id, string assignmentId)
        {
            return Get(Call("module",
                new[] { new Parameter("id", id), new Parameter("assignmentId", assignmentId) },
                stream => (NetDimensions.Apis.Module.module)new XmlSerializer(typeof(NetDimensions.Apis.Module.module)).Deserialize(stream)));
        }

        public competencies GetCompetenciesAwarded(string assignmentId)
        {
            return Get(Call("competenciesAwarded",
                new[] { new Parameter("assignmentId", assignmentId) },
                stream => (competencies)new XmlSerializer(typeof(competencies)).Deserialize(stream)));
        }

        private class WebClient : Client
        {
            private readonly string baseUrl;
            private readonly NetworkCredential credentials;
            private readonly string onBehalfOf;
            internal WebClient(string url, NetworkCredential credentials, string onBehalfOf)
            {
                this.baseUrl = url;
                this.credentials = credentials;
                this.onBehalfOf = onBehalfOf;
            }

            protected override T Get<T>(Call<T> call)
            {
                WebRequest req = WebRequest.Create(baseUrl + "api/" + call.FunctionName + "?"
                    + Parameter.ToString(call.Parameters.Concat(new[] { new Parameter("onBehalfOf", onBehalfOf) })));
                req.Credentials = credentials;
                using (WebResponse resp = req.GetResponse())
                {
                    return call.ResponseParser(resp.GetResponseStream());
                }
            }
        };

        public static Client From(string url, NetworkCredential credentials, string onBehalfOf)
        {
            return new WebClient(url, credentials, onBehalfOf);
        }
    }
}
