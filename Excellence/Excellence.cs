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
    internal class Result
    {
        internal bool IsRevised { get; private set; }
        internal IEnumerable<Unit> Units { get; private set; }
        internal Result(bool isRevised, IEnumerable<Unit> units)
        {
            IsRevised = isRevised;
            Units = units;
        }
    }

    public class Excellence
    {
        private readonly Client client;
        private readonly string assignmentId;
        public Excellence(Client client, string assignmentId)
        {
            this.client = client;
            this.assignmentId = assignmentId;
        }

        static void Main(string[] args)
        {
            // Create generic API client instance
            Client client = Client.From(args[0], new NetworkCredential(args[1], args[2]), args[3]);

            // Create Excellence wrapper
            string assignmentId = args[4];
            Excellence ex = new Excellence(client, assignmentId);

            // Get the expanded learning path, and do something with it
            learningPath path = ex.GetExpandedLearningPath();
            foreach (jobProfile p in path.jobProfile)
            {
                foreach (competency c in p.competency)
                {
                    if (c.sequence != null)
                    {
                        foreach (sequence s in c.sequence)
                        {
                            foreach (item i in s.item)
                            {
                                // This could be an unenrolled program
                                if (i.sequence != null)
                                {
                                    foreach (sequence s2 in i.sequence)
                                    {
                                        foreach (item i2 in s2.item)
                                        {
                                            Console.WriteLine("Submodule: {0}", i2.module.title);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.ReadKey();
        }

        // Consolidating schema would make this unnecessary
        private static typeCode ConvertType(NetDimensions.Apis.Module.type t)
        {
            switch (t)
            {
                case NetDimensions.Apis.Module.type.archivedVirtualClassroom:
                    return typeCode.archivedVirtualClassroom;
                case NetDimensions.Apis.Module.type.audioCassette:
                    return typeCode.audioCassette;
                case NetDimensions.Apis.Module.type.book:
                    return typeCode.book;
                case NetDimensions.Apis.Module.type.cd:
                    return typeCode.cd;
                case NetDimensions.Apis.Module.type.classroom:
                    return typeCode.classroom;
                case NetDimensions.Apis.Module.type.coaching:
                case NetDimensions.Apis.Module.type.exam:
                case NetDimensions.Apis.Module.type.internship:
                case NetDimensions.Apis.Module.type.onlineModule:
                case NetDimensions.Apis.Module.type.onTheJobTraining:
                case NetDimensions.Apis.Module.type.selfTrainingPaper:
                case NetDimensions.Apis.Module.type.selfTrainingVideo:
                case NetDimensions.Apis.Module.type.specialInterestGroup:
                case NetDimensions.Apis.Module.type.task:
                default:
                    return typeCode.onlineModule;
                case NetDimensions.Apis.Module.type.externalCertification:
                    return typeCode.externalCertification;
                case NetDimensions.Apis.Module.type.externalCourse:
                    return typeCode.externalCourse;
                case NetDimensions.Apis.Module.type.justInTimeLearning:
                    return typeCode.justInTimeLearning;
                case NetDimensions.Apis.Module.type.learningProgram:
                    return typeCode.learningProgram;
                case NetDimensions.Apis.Module.type.video:
                    return typeCode.video;
                case NetDimensions.Apis.Module.type.virtualClassroom:
                    return typeCode.virtualClassroom;
                case NetDimensions.Apis.Module.type.workshopSeminar:
                    return typeCode.workshopSeminar;
            }
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
                                                        code = ConvertType(m.type),
                                                        label = m.typeLabel
                                                    }
                                                },
                                                url = m.link[1].href
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

        private IEnumerable<UnitRefreshCompetency> UnitRefreshCompetencies(competency[] cs)
        {
            return from c in cs select new UnitRefreshCompetency(c, new object[0]);
        }

        public Task<Result> GetUnits(jobProfile p, competency c, item i)
        {
            Task<bool> isRevised = Task.Run(() =>
            {
                if (i.module.type.code == typeCode.onlineModule)
                {
                    return !"0".Equals(client.GetModule(i.module.id, assignmentId).effectiveRevision);
                }
                else
                {
                    return false;
                }
            });
            Task<IEnumerable<Unit>> units = GetUnits(p, c, i.sequence);
            return Task.Run(() =>
                {
                    return new Result(isRevised.Result, units.Result);
                });
        }

        public Task<IEnumerable<Unit>> GetUnits(jobProfile p, competency c, sequence s)
        {
            List<Task<IEnumerable<Unit>>> tasks = new List<Task<IEnumerable<Unit>>>();
            foreach (item i in s.item)
            {
                tasks.Add(GetUnits(p, c, i));
            }
            return Task.WhenAll(tasks).ContinueWith((lists) => lists.Result.SelectMany(e => e));
        }

        public Task<IEnumerable<Unit>> GetUnits(jobProfile p, competency c)
        {
            List<Task<IEnumerable<Unit>>> tasks = new List<Task<IEnumerable<Unit>>>();
            foreach (sequence s in c.sequence)
            {
                tasks.Add(GetUnits(p, c, s));
            }
            return Task.WhenAll(tasks).ContinueWith((lists) => lists.Result.SelectMany(e => e));
        }

        public Task<IEnumerable<Unit>> GetUnits(jobProfile p)
        {
            List<Task<IEnumerable<Unit>>> tasks = new List<Task<IEnumerable<Unit>>>();
            foreach (competency c in p.competency)
            {
                tasks.Add(GetUnits(p, c));
            }
            return Task.WhenAll(tasks).ContinueWith((lists) => lists.Result.SelectMany(e => e));
        }

        public Task<IEnumerable<Unit>> GetUnitRefreshAsync()
        {
            var t1 = Task.Run(() => client.GetLearningPath(assignmentId));
            var t2 = Task.Run(() => client.GetRecords());
            List<Task<IEnumerable<Unit>>> tasks = new List<Task<IEnumerable<Unit>>>();
            foreach (jobProfile p in t1.Result.jobProfile)
            {
                tasks.Add(GetUnits(p));
            }
            return Task.WhenAll(tasks).ContinueWith((lists) => lists.Result.SelectMany(e => e));
        }
    }

    public class Unit
    {

    }

    public class UnitRefreshJobProfile
    {
        public jobProfile JobProfile { get; private set; }
        public IEnumerable<UnitRefreshCompetency> Competencies { get; private set; }
        internal UnitRefreshJobProfile(jobProfile p, IEnumerable<UnitRefreshCompetency> c)
        {
            JobProfile = p;
            Competencies = c;
        }
    }

    public class UnitRefreshCompetency
    {
        public competency Competency { get; private set; }
        public IEnumerable<object> Modules { get; private set; }
        internal UnitRefreshCompetency(competency c, IEnumerable<object> m)
        {
            Competency = c;
            Modules = m;
        }
    }
}
