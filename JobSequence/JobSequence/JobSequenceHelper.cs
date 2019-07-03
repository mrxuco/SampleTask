using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JobSequence
{
    public static class JobSequenceHelper
    {
        /// <summary>
        /// Returns job execution sequences
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<List<string>> GetJobSequence(string input)
        {
            //clear input
            input = input.Replace(" ", string.Empty);

            var jobs = new ConcurrentDictionary<string, string>();

            //fill jobs with input
            using (var textreader = new StringReader(input))
            {
                string line = textreader.ReadLine();

                while (line != null)
                {
                    var tuple = line.Split(new string[] { "=>" }, StringSplitOptions.None);

                    if (tuple.First() == tuple.Last()) throw new Exception("Self dependency detected");

                    if (!string.IsNullOrEmpty(tuple.First())) jobs.TryAdd(tuple.First(), tuple.Last());
                    line = textreader.ReadLine();
                }
            }

            //result
            var dependencyList = new Dictionary<string, List<string>>();

            foreach (var x in jobs)
            {
                dependencyList.Add(x.Key, GetJobsPredecessors(x.Key, jobs, new List<string>()));
            }

            return dependencyList.Select(x => x.Value).ToList();
        }

        private static List<string> GetJobsPredecessors(string job, ConcurrentDictionary<string, string> complexJobs, List<string> predecessors)
        {
            if (predecessors == null) predecessors = new List<string>();

            //get dependency 
            var predecessor = complexJobs.FirstOrDefault(x => x.Key != job && x.Value.Contains(job));

            //if no more dependency found, return collected result
            if (predecessor.Key == null)
            {
                predecessors.Reverse();
                return predecessors;
            }

            predecessors.Add(job);

            if (predecessors.Contains(predecessor.Key)) throw new Exception("Circular reference");

            //continue finding predecessors
            return GetJobsPredecessors(predecessor.Key, complexJobs, predecessors);
        }
    }
}