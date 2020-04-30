using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Slugify.Core.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SlugifyBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class SlugifyBenchmarks
    {
        private SlugHelper _slugHelper;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _slugHelper = new SlugHelper();
        }

        [Params(1)]
        public int N;

        [Params("Hello", "lorem & ipsome Some Other thing!_ woot!!")]
        public string word;

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            for (int i = 0; i < N; i++)
            {
                _slugHelper.GenerateSlug(word);
            }
        }

        [Benchmark]
        public void Improved()
        {
            for (int i = 0; i < N; i++)
            {
                _slugHelper.GenerateSlug2(word);
            }
        }


    }
}
