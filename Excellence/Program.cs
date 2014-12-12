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
            learningPath path = new NetDimensions.Apis.Client(args[0],
                new NetworkCredential(args[1], args[2]),
                args[3]).getLearningPath(args[4]);
            foreach (jobProfile p in path.jobProfile)
            {
                Console.WriteLine("Job profile: {0}", p.name);
            }
            Console.ReadKey();
        }
    }
}
