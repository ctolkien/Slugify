``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.778 (1909/November2018Update/19H2)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.1.201
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT


```
|   Method |     Mean |    Error |   StdDev | Ratio |     Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------- |---------:|---------:|---------:|------:|----------:|------:|------:|----------:|
| Baseline | 42.32 ms | 0.045 ms | 0.040 ms |  1.00 | 3750.0000 |     - |     - |  30.57 MB |
| Improved | 26.14 ms | 0.323 ms | 0.287 ms |  0.62 | 2562.5000 |     - |     - |   20.6 MB |
