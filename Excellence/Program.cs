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
            WebRequest.Create(baseUrl + "api/learningPath?"+ ToString("format", "xml") + "&" + ToString("onBehalfOf", onBehalfOf));
            System.IO.FileStream x = new System.IO.FileStream("C:\\Users\\robertlowe\\Source\\Repos\\Excellence\\learningPath.xml", System.IO.FileMode.Open);
            learningPath path = (learningPath)new System.Xml.Serialization.XmlSerializer(typeof(NetDimensions.Apis.LearningPath.learningPath)).Deserialize(x);
            foreach (jobProfile p in path.jobProfile)
            {
                Console.WriteLine("Job profile: {0}", p.name);
            }
            Console.ReadKey();
        }

        private static string ToString(string name, string value)
        {
            return Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value);
        }
    }
}
