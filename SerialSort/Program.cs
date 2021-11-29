using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace SerialSort
{
    public class Program
    {
        public static void FillArray(int[] unsortedArray)
        {
            Random rand = new Random();
            for (int i = 0; i < unsortedArray.Length; i++)
            {
                unsortedArray[i] = rand.Next(0, 1000000);
            }
        }

        [MemoryDiagnoser()]
        [Orderer(SummaryOrderPolicy.FastestToSlowest)]
        [RankColumn()]
        [RPlotExporter]
        [SimpleJob(RunStrategy.Throughput)]
        public class BenchmarkTest
        {
            [Params(1000000, 10000000, 100000000)]
            public int N = 1000000;

            [GlobalSetup]
            public void Setup()
            {
                unsortedArray = new int[N];
                FillArray(unsortedArray);
            }
            
            public int[] unsortedArray;

            [Benchmark]
            public void ParallelTest()
            {
                ParallelSort.ShellSort(unsortedArray);
            }

            // Console.WriteLine("Отсортированный массив: {0}", string.Join(", ", unsortedArray));

            [Benchmark]
            public void SerialTest()
            {
                SerialSort.ShellSort(unsortedArray);
            }

            // Console.WriteLine("Отсортированный массив: {0}", string.Join(", ", unsortedArray));
        }
        static void Main(string[] args)
        {
           // ParallelSort parallelSort = new ParallelSort();
           // parallelSort.QuickBucketSort();
            var summary = BenchmarkRunner.Run<ParallelSort>();
            // var summary = BenchmarkRunner.Run<SerialSort>();
        }
    }
}