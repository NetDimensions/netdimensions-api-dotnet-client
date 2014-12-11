using NetDimensions.Apis.LearningPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

        public static string ToString(params Parameter[] parameters)
        {
            return string.Join("&", (object[])parameters);
        }
    }

    delegate T Parser<T>(System.IO.Stream str);

    class Client
    {
        private readonly string baseUrl;
        private readonly NetworkCredential credentials;
        public Client(string url, NetworkCredential cred)
        {
            this.baseUrl = url;
            this.credentials = cred;
        }

        public learningPath getLearningPath(string onBehalfOf)
        {
            return Get("learningPath",
                new[] { new Parameter("format", "xml"), new Parameter("onBehalfOf", onBehalfOf) },
                stream => (learningPath)new System.Xml.Serialization.XmlSerializer(typeof(NetDimensions.Apis.LearningPath.learningPath)).Deserialize(stream));
        }

        private T Get<T>(string functionName, Parameter[] parameters, Parser<T> responseParser)
        {
            WebRequest req = WebRequest.Create(baseUrl + "api/" + functionName + "?" + Parameter.ToString(parameters));
            req.Credentials = credentials;
            using (WebResponse resp = req.GetResponse())
            {
                return responseParser(resp.GetResponseStream());
            }
        }
    }
}
