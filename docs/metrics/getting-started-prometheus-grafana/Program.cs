// <copyright file="Program.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

public class Program
{
    private static readonly Meter MyMeter = new("MyCompany.MyProduct.MyLibrary", "1.0");
    private static readonly Counter<long> MyFruitCounter = MyMeter.CreateCounter<long>("MyFruitCounter");

    public static void Main()
    {
        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("MyCompany.MyProduct.MyLibrary")
            .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
            {
                exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
            })
            .Build();

        Console.WriteLine("Press any key to exit");

        while (!Console.KeyAvailable)
        {
            MyFruitCounter.Add(1, new("name", "apple"), new("color", "red"));
            MyFruitCounter.Add(2, new("name", "lemon"), new("color", "yellow"));
            MyFruitCounter.Add(1, new("name", "lemon"), new("color", "yellow"));
            MyFruitCounter.Add(2, new("name", "apple"), new("color", "green"));
            MyFruitCounter.Add(5, new("name", "apple"), new("color", "red"));
            MyFruitCounter.Add(4, new("name", "lemon"), new("color", "yellow"));

            Thread.Sleep(300);
        }

        // Dispose meter provider before the application ends.
        // This will flush the remaining metrics and shutdown the metrics pipeline.
        meterProvider?.Dispose();
    }
}
