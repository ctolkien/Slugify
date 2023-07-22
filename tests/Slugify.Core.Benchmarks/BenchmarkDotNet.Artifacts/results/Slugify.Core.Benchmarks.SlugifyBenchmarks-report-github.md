```

BenchmarkDotNet v0.13.6, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100-preview.6.23330.14
  [Host]     : .NET 6.0.20 (6.0.2023.32017), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.20 (6.0.2023.32017), X64 RyuJIT AVX2


```
|   Method |     Mean |    Error |   StdDev | Ratio | RatioSD |      Gen0 | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|--------:|----------:|----------:|------------:|
| Baseline | 29.73 ms | 0.584 ms | 0.799 ms |  1.00 |    0.00 | 2718.7500 |   21.8 MB |        1.00 |
| Improved | 23.56 ms | 0.457 ms | 0.449 ms |  0.79 |    0.03 |  593.7500 |   4.85 MB |        0.22 |
| NonAscii | 25.79 ms | 0.475 ms | 0.467 ms |  0.87 |    0.03 |  718.7500 |   5.85 MB |        0.27 |
