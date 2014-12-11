using NetDimensions.Apis.LearningPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Excellence
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseUrl = "https://preview.netdimensions.com/preview/";
            string onBehalfOf = "netd_rob";
            string requestUriString = baseUrl + "api/learningPath?" + Parameter.ToString(new Parameter("format", "xml"), new Parameter("onBehalfOf", onBehalfOf));
            Console.WriteLine(requestUriString);
            WebRequest req = WebRequest.Create(requestUriString);
            req.Credentials = ;
            req.GetResponse();
            System.IO.FileStream x = new System.IO.FileStream("C:\\Users\\robertlowe\\Source\\Repos\\Excellence\\learningPath.xml", System.IO.FileMode.Open);
            learningPath path = (learningPath)new System.Xml.Serialization.XmlSerializer(typeof(NetDimensions.Apis.LearningPath.learningPath)).Deserialize(x);
            foreach (jobProfile p in path.jobProfile)
            {
                Console.WriteLine("Job profile: {0}", p.name);
            }
            Console.ReadKey();
        }
    }

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
}
