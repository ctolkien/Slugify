```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3476)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.201
  [Host]   : .NET 8.0.14 (8.0.1425.11118), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.14 (8.0.1425.11118), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method              | Mean     | Error    | StdDev    | Ratio | RatioSD | Gen0     | Gen1   | Allocated | Alloc Ratio |
|-------------------- |---------:|---------:|----------:|------:|--------:|---------:|-------:|----------:|------------:|
| OldVersion          | 17.346 ms | 0.7337 ms | 0.0402 ms |  2.40 |    0.03 | 562.5000 |      - |   4.64 MB |        0.84 |
| Baseline            | 7.299 ms | 1.097 ms | 0.0602 ms |  1.00 |    0.01 | 687.5000 | 7.8125 |   5.51 MB |        1.00 |
| BaselineBetterRegex | 6.278 ms | 5.947 ms | 0.3260 ms |  0.86 |    0.04 | 687.5000 |      - |   5.51 MB |        1.00 |
| Standard            | 7.163 ms | 2.939 ms | 0.1611 ms |  0.98 |    0.02 | 460.9375 |      - |    3.7 MB |        0.67 |
| NonAscii            | 7.236 ms | 1.180 ms | 0.0647 ms |  0.99 |    0.01 | 585.9375 |      - |    4.7 MB |        0.85 |
