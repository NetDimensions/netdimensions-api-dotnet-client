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
    public class Excellence
    {
        private readonly Client client;
        private readonly string assignmentId;
        public Excellence(Client client, string assignmentId)
        {
            this.client = client;
            this.assignmentId = assignmentId;
        }

        void Main()
        {
            learningPath path = client.GetLearningPath(assignmentId);
            foreach (jobProfile p in path.jobProfile)
            {
                Console.WriteLine("Job profile: {0}", p.name);
            }
            foreach (trainingRecord r in client.GetRecords().trainingRecord)
            {
                Console.WriteLine("Record: {0} ({1})", r.learningModule.title, r.revision);
            }
            NetDimensions.Apis.Module.module mod = client.GetModule("SingleCourseManifest", assignmentId);
            Console.WriteLine("ID: {0}", mod.id);
            Console.WriteLine("Title: {0}", mod.title);
            Console.WriteLine("Revision: {0}", mod.effectiveRevision);
            client.GetCompetenciesAwarded(assignmentId);
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Client client = Client.From(args[0], new NetworkCredential(args[1], args[2]), args[3]);
            string assignmentId = args[4];
            new Excellence(client, assignmentId).Main();
        }

        private IEnumerable<Task> ExpandSequence(sequence[] ss)
        {
            var result = new List<Task>();
            if (ss != null)
            {
                foreach (sequence s in ss)
                {
                    foreach (item i in s.item)
                    {
                        if (i.sequence == null)
                        {
                            if (i.module.type.code == typeCode.learningProgram)
                            {
                                result.Add(Task.Run(() =>
                                {
                                    var modules = client.GetModule(i.module.id, assignmentId).session[0]
                                        .module;
                                    i.sequence = (from m in modules select new sequence {
                                        item = new item[] {
                                            new item {
                                                status = i.status,
                                                module = new module {
                                                    id = m.id,
                                                    title = m.title,
                                                    type = new NetDimensions.Apis.LearningPath.type {
                                                        label = m.typeLabel
                                                    }
                                                }
                                            }
                                        }
                                    }).ToArray();
                                }));
                            }
                        }
                        else
                        {
                            result.AddRange(ExpandSequence(i.sequence));
                        }
                    }
                }
            }
            return result;
        }

        public learningPath GetExpandedLearningPath()
        {
            learningPath path = client.GetLearningPath(assignmentId);
            var tasks = new List<Task>();
            foreach (jobProfile p in path.jobProfile)
            {
                foreach (competency c in p.competency)
                {
                    tasks.AddRange(ExpandSequence(c.sequence));
                }
            }
            Task.WaitAll(tasks.ToArray());
            return path;
        }
    }
}
