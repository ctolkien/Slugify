using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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
public partial class SlugifyBenchmarks
{
    private List<string> _textList;
    private SlugHelper _slugHelper;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _textList = [.. File.ReadAllLines("gistfile.txt")];
        _slugHelper = new SlugHelper();

    }

    [Benchmark(Baseline = true)]
    public void Baseline()
    {
        var sluggy = new SlugHelper(new SlugHelperConfiguration
        {
			// to enable legacy behaviour, for fairness
			DeniedCharactersRegex = new(@"[^a-zA-Z0-9\-\._]")
        });
        for (var i = 0; i < _textList.Count; i++)
        {
            sluggy.GenerateSlug(_textList[i]);
        }
    }

	[Benchmark]
	public void BaselineBetterRegex()
	{
		var sluggy = new SlugHelper(new SlugHelperConfiguration
		{
			// to enable legacy behaviour, for fairness
			DeniedCharactersRegex = GeneratedRegex()
		});
		for (var i = 0; i < _textList.Count; i++)
		{
			sluggy.GenerateSlug(_textList[i]);
		}
	}

	[Benchmark]
    public void Standard()
    {
        for (var i = 0; i < _textList.Count; i++)
        {
            _slugHelper.GenerateSlug(_textList[i]);
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

	[GeneratedRegex(@"[^a-z0-9\-\._]")]
	private static partial Regex GeneratedRegex();
}
