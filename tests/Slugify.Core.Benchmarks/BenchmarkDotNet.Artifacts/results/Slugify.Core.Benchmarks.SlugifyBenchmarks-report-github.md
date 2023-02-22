``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1265/22H2/2022Update/SunValley2)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.200
  [Host]     : .NET 6.0.14 (6.0.1423.7309), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.14 (6.0.1423.7309), X64 RyuJIT AVX2


```
|   Method |     Mean |    Error |   StdDev | Ratio |      Gen0 | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|----------:|----------:|------------:|
| Baseline | 28.87 ms | 0.441 ms | 0.412 ms |  1.00 | 2718.7500 |   21.8 MB |        1.00 |
| Improved | 23.64 ms | 0.206 ms | 0.193 ms |  0.82 |  593.7500 |   4.85 MB |        0.22 |
