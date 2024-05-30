using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.IO;

namespace Slugify.Core.Benchmarks;

internal class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<SlugifyBenchmarks>();
    }
}

[ShortRunJob]
[MemoryDiagnoser]
public class SlugifyBenchmarks
{
    private List<string> _textList;
    private SlugHelper _slugHelper;
    private RevisedSlugHelper _improvedSlugHelper;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _textList = [.. File.ReadAllLines("gistfile.txt")];
        _slugHelper = new SlugHelper();
        _improvedSlugHelper = new RevisedSlugHelper();

    }

    [Benchmark(Baseline = true)]
    public void Baseline()
    {
        var sluggy = new SlugHelper(new SlugHelperConfiguration
        {
            // to enable legacy behaviour, for fairness
            DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]"
        });
        for (var i = 0; i < _textList.Count; i++)
        {
            sluggy.GenerateSlug(_textList[i]);
        }
    }

    [Benchmark]
    public void DWengier()
    {
        for (var i = 0; i < _textList.Count; i++)
        {
            _slugHelper.GenerateSlug(_textList[i]);
        }
    }

    [Benchmark]
    public void Copilot()
    {
        for (var i = 0; i < _textList.Count; i++)
        {
            _improvedSlugHelper.GenerateSlug(_textList[i]);
        }
    }

    [Benchmark]
    public void NonAscii()
    {
        var helper = new SlugHelperForNonAsciiLanguages();
        for (var i = 0; i < _textList.Count; i++)
        {
            helper.GenerateSlug(_textList[i]);
        }
    }
}
