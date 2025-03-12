```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.4037/23H2/2023Update/SunValley3)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.100-preview.7.24407.12
  [Host]   : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method              | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1   | Allocated | Alloc Ratio |
|-------------------- |----------:|----------:|----------:|------:|--------:|---------:|-------:|----------:|------------:|
| Baseline            |  7.227 ms | 1.5496 ms | 0.0849 ms |  1.00 |    0.00 | 687.5000 | 7.8125 |   5.51 MB |        1.00 |
| OldVersion          | 17.346 ms | 0.7337 ms | 0.0402 ms |  2.40 |    0.03 | 562.5000 |      - |   4.64 MB |        0.84 |
| BaselineBetterRegex |  6.402 ms | 3.8224 ms | 0.2095 ms |  0.89 |    0.04 | 687.5000 |      - |   5.51 MB |        1.00 |
| Standard            |  7.735 ms | 1.1115 ms | 0.0609 ms |  1.07 |    0.02 | 453.1250 |      - |    3.7 MB |        0.67 |
| NonAscii            |  8.024 ms | 0.6855 ms | 0.0376 ms |  1.11 |    0.01 | 578.1250 |      - |    4.7 MB |        0.85 |
