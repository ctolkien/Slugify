```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22621.3672/22H2/2022Update/SunValley2)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.301
  [Host]   : .NET 8.0.6 (8.0.624.26715), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.6 (8.0.624.26715), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method   | Mean      | Error    | StdDev    | Ratio | RatioSD | Gen0     | Allocated | Alloc Ratio |
|--------- |----------:|---------:|----------:|------:|--------:|---------:|----------:|------------:|
| Baseline | 14.994 ms | 1.990 ms | 0.1091 ms |  1.00 |    0.00 | 953.1250 |   7.65 MB |        1.00 |
| DWengier | 17.520 ms | 5.280 ms | 0.2894 ms |  1.17 |    0.01 | 562.5000 |   4.64 MB |        0.61 |
| Copilot  |  7.760 ms | 2.762 ms | 0.1514 ms |  0.52 |    0.01 | 460.9375 |    3.7 MB |        0.48 |
| NonAscii | 18.341 ms | 6.463 ms | 0.3543 ms |  1.22 |    0.02 | 687.5000 |   5.64 MB |        0.74 |
