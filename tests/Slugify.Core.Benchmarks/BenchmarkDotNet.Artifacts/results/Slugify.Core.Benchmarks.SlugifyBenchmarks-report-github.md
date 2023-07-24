```

BenchmarkDotNet v0.13.6, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100-preview.6.23330.14
  [Host]   : .NET 6.0.20 (6.0.2023.32017), X64 RyuJIT AVX2
  ShortRun : .NET 6.0.20 (6.0.2023.32017), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|          Method |     Mean |     Error |   StdDev | Ratio | RatioSD |      Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|----------:|---------:|------:|--------:|----------:|----------:|------------:|
|        Baseline | 32.09 ms | 15.438 ms | 0.846 ms |  1.00 |    0.00 | 2687.5000 |   21.8 MB |        1.00 |
|        Improved | 24.13 ms |  7.909 ms | 0.433 ms |  0.75 |    0.03 |  593.7500 |   4.85 MB |        0.22 |
| ImprovedReusing | 23.99 ms | 10.798 ms | 0.592 ms |  0.75 |    0.03 |  562.5000 |   4.64 MB |        0.21 |
|        NonAscii | 26.22 ms | 16.970 ms | 0.930 ms |  0.82 |    0.01 |  718.7500 |   5.85 MB |        0.27 |
| NonAsciiReusing | 26.24 ms |  2.172 ms | 0.119 ms |  0.82 |    0.02 |  687.5000 |   5.64 MB |        0.26 |
