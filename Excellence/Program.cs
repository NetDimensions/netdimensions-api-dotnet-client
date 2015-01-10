using NetDimensions.Apis;
using NetDimensions.Apis.LearningPath;
using NetDimensions.Apis.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetDimensions.Excellence
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(args[0], new NetworkCredential(args[1], args[2]), args[3]);
            string assignmentId = args[4];
            learningPath path = client.getLearningPath(assignmentId);
            foreach (jobProfile p in path.jobProfile)
            {
                Console.WriteLine("Job profile: {0}", p.name);
            }
            foreach (trainingRecord r in client.getRecords().trainingRecord)
            {
                Console.WriteLine("Record: {0} ({1})", r.learningModule.title, r.revision);
            }
            NetDimensions.Apis.Module.module mod = client.getModule("SingleCourseManifest", assignmentId);
            Console.WriteLine("ID: {0}", mod.id);
            Console.WriteLine("Title: {0}", mod.title);
            Console.WriteLine("Revision: {0}", mod.effectiveRevision);
            client.getCompetenciesAwarded(assignmentId);
            Console.ReadKey();
        }
    }
}
