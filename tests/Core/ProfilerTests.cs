using Xunit;
using GameFramework.Core;
using System.Threading;

namespace GameFramework.Tests.Core
{
    public class ProfilerTests
    {
        [Fact]
        public void Profiler_StartStop_RecordsTime()
        {
            var profiler = new Profiler();
            string sectionName = "TestSection";

            profiler.Start(sectionName);
            Thread.Sleep(50); // Simulate work
            profiler.Stop(sectionName);

            var section = profiler.GetSection(sectionName);

            Assert.NotNull(section);
            Assert.True(section.LastElapsedTimeMilliseconds >= 50);
            Assert.Single(section.Samples);
            Assert.True(section.AverageElapsedTimeMilliseconds >= 50);
        }

        [Fact]
        public void Profiler_MultipleSections_RecordsTimeIndependently()
        {
            var profiler = new Profiler();
            string sectionName1 = "Section1";
            string sectionName2 = "Section2";

            profiler.Start(sectionName1);
            Thread.Sleep(30);
            profiler.Stop(sectionName1);

            profiler.Start(sectionName2);
            Thread.Sleep(60);
            profiler.Stop(sectionName2);

            var section1 = profiler.GetSection(sectionName1);
            var section2 = profiler.GetSection(sectionName2);

            Assert.NotNull(section1);
            Assert.True(section1.LastElapsedTimeMilliseconds >= 30);
            Assert.Single(section1.Samples);

            Assert.NotNull(section2);
            Assert.True(section2.LastElapsedTimeMilliseconds >= 60);
            Assert.Single(section2.Samples);
        }

        [Fact]
        public void Profiler_RollingAverage_CalculatesCorrectly()
        {
            var profiler = new Profiler();
            string sectionName = "RollingAverageSection";

            profiler.Start(sectionName);
            Thread.Sleep(10);
            profiler.Stop(sectionName);

            profiler.Start(sectionName);
            Thread.Sleep(20);
            profiler.Stop(sectionName);

            var section = profiler.GetSection(sectionName);
            Assert.NotNull(section);
            Assert.Equal(2, section.Samples.Count);
            Assert.True(section.AverageElapsedTimeMilliseconds >= 15 && section.AverageElapsedTimeMilliseconds < 20); // Average of 10 and 20 is 15
        
            // Fill up samples to test rolling nature
            for (int i = 0; i < 100; i++)
            {
                profiler.Start(sectionName);
                Thread.Sleep(1); // Minimal time
                profiler.Stop(sectionName);
            }
            Assert.Equal(100, section.Samples.Count); // MaxSamples + initial 2, then one removed, so 100
            Assert.True(section.Samples[0] >=1 ); // First sample should be one of the 1ms sleeps
        }

        [Fact]
        public void Profiler_GetNonExistentSection_ReturnsNull()
        {
            var profiler = new Profiler();
            var section = profiler.GetSection("NonExistentSection");
            Assert.Null(section);
        }
    }
}
