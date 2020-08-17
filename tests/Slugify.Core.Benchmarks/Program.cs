using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Slugify.Core.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<SlugifyBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class SlugifyBenchmarks
    {
        private List<string> _textList;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _textList = File.ReadAllLines("gistfile.txt").ToList();
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            for (var i = 0; i < _textList.Count; i++)
            {
                new SlugHelperLegacy(new SlugHelperLegacy.Config
                {
                    // to enable legacy behaviour, for fairness
                    DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]"
                }).GenerateSlug(_textList[i]);
            }
        }

        [Benchmark]
        public void Improved()
        {
            for (var i = 0; i < _textList.Count; i++)
            {
                new SlugHelper().GenerateSlug(_textList[i]);
            }
        }
    }
}
