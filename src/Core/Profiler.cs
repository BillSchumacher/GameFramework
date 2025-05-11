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

        public string GetFormattedOutput(string title, bool forConsole)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine(new string('-', title.Length > 0 ? title.Length : 10)); // Separator

            foreach (var section in Sections.OrderBy(s => s.Name))
            {
                if (forConsole)
                {
                    // Pad section name for alignment in console
                    sb.AppendLine($"  {section.Name,-30}: Avg {section.AverageElapsedTimeMilliseconds,4} ms (Last: {section.LastElapsedTimeMilliseconds,4} ms)");
                }
                else // For in-game label (this specific formatting might be per-label now)
                {
                    sb.AppendLine($"{section.Name}: {section.AverageElapsedTimeMilliseconds}ms (L: {section.LastElapsedTimeMilliseconds}ms)");
                }
            }
            return sb.ToString();
        }
    }
}
