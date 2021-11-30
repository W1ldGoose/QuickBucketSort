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
                unsortedArray[i] = rand.Next(0, 100000);
            }
        }

        public static int[] testArr;

        [MemoryDiagnoser()]
        [Orderer(SummaryOrderPolicy.FastestToSlowest)]
        [RankColumn()]
       // [RPlotExporter]
        // Usually, you shouldn't specify such characteristics like LaunchCount, WarmupCount, TargetCount, or IterationTime
        // because BenchmarkDotNet has a smart algorithm to choose these values automatically based on received measurements.
        [SimpleJob(RunStrategy.Throughput)]
        public class BenchmarkTest
        {
            [Params( 1000)] 
            public int N = 1000;

            public int[] unsortedArray;

            [GlobalSetup]
            public void Setup()
            {
                unsortedArray = new int[N];
                FillArray(unsortedArray);
            }

            [Benchmark]
            public void ParallelTest() => QBucketSort.ParallelQBucketSort(unsortedArray);

            [Benchmark]
            public void SerialTest() => QBucketSort.SerialQBucketSort(unsortedArray);
        }

        static void Main(string[] args)
        {
            /*testArr = new int[100];
            FillArray(testArr);
            QBucketSort.SerialQBucketSort(testArr);
            foreach (var i in testArr)
            {
                Console.Write(i+" ");
            }*/
           var summary = BenchmarkRunner.Run<BenchmarkTest>();
        }
    }
}