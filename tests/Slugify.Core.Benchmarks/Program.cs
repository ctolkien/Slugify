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
            var summary = BenchmarkRunner.Run<SlugifyBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class SlugifyBenchmarks
    {
        private ISlugHelper _slugHelper;
        private ISlugHelper _slugHelperImproved;
        private List<string> _textList;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _slugHelper = new SlugHelper();
            _slugHelperImproved = new SlugHelperImproved();
            _textList = File.ReadAllLines("gistfile.txt").ToList();
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            for (var i = 0; i < _textList.Count; i++)
            {
                _slugHelper.GenerateSlug(_textList[i]);
            }
        }

        [Benchmark]
        public void Improved()
        {
            for (var i = 0; i < _textList.Count; i++)
            {
                _slugHelperImproved.GenerateSlug(_textList[i]);
            }
        }
    }
}
