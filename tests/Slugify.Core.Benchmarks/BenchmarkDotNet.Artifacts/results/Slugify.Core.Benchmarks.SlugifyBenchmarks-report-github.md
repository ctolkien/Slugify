``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.778 (1909/November2018Update/19H2)
Intel Core i9-9900K CPU 3.60GHz (Coffee Lake), 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.1.201
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT


```
|   Method | N |                 word |       Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------- |-- |--------------------- |-----------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
| **Baseline** | **1** |                **Hello** |   **660.0 ns** |  **2.01 ns** |  **1.78 ns** |  **1.00** |    **0.00** | **0.0734** |     **-** |     **-** |     **616 B** |
| Improved | 1 |                Hello |   643.8 ns | 12.74 ns | 11.92 ns |  0.98 |    0.02 | 0.0553 |     - |     - |     464 B |
|          |   |                      |            |          |          |       |         |        |       |       |           |
| **Baseline** | **1** | **lorem(...)oot!! [40]** | **4,174.1 ns** | **11.46 ns** | **10.72 ns** |  **1.00** |    **0.00** | **0.4501** |     **-** |     **-** |    **3824 B** |
| Improved | 1 | lorem(...)oot!! [40] | 3,924.9 ns |  7.03 ns |  5.87 ns |  0.94 |    0.00 | 0.3967 |     - |     - |    3360 B |
