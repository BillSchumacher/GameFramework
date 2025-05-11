using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameFramework.Core
{
    public class ProfilerSection
    {
        public string Name { get; }
        public Stopwatch Stopwatch { get; } = new Stopwatch();
        public long LastElapsedTimeMilliseconds => Stopwatch.ElapsedMilliseconds;
        public List<long> Samples { get; } = new List<long>();
        public long AverageElapsedTimeMilliseconds => Samples.Count > 0 ? (long)Samples.Average() : 0;
        private const int MaxSamples = 100; // Keep a rolling average of the last 100 samples

        public ProfilerSection(string name)
        {
            Name = name;
        }

        public void Start()
        {
            Stopwatch.Restart();
        }

        public void Stop()
        {
            Stopwatch.Stop();
            Samples.Add(Stopwatch.ElapsedMilliseconds);
            if (Samples.Count > MaxSamples)
            {
                Samples.RemoveAt(0); // Keep the list size manageable
            }
        }
    }

    public class Profiler
    {
        private readonly Dictionary<string, ProfilerSection> _sections = new Dictionary<string, ProfilerSection>();
        public IEnumerable<ProfilerSection> Sections => _sections.Values;

        public void Start(string sectionName)
        {
            if (!_sections.TryGetValue(sectionName, out var section))
            {
                section = new ProfilerSection(sectionName);
                _sections[sectionName] = section;
            }
            section.Start();
        }

        public void Stop(string sectionName)
        {
            if (_sections.TryGetValue(sectionName, out var section))
            {
                section.Stop();
            }
        }

        public ProfilerSection? GetSection(string sectionName)
        {
            _sections.TryGetValue(sectionName, out var section);
            return section;
        }
    }
}
