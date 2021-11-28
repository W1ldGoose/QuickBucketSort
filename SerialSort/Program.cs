using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using Microsoft.Diagnostics.Runtime.Interop;

namespace SerialSort
{
    [RPlotExporter]
    class Program
    {
        public class MyConfig : ManualConfig
        {
            public MyConfig()
            {
                AddExporter(CsvMeasurementsExporter.Default);
                AddExporter(RPlotExporter.Default);
                Add(Job.Default.With(ClrRuntime.Net47).With(Jit.RyuJit).With(Platform.X64).WithId("NET4.7_RyuJIT-x64"));
                //  Add(CsvMeasurementsExporter.Default);
                //  Add(RPlotExporter.Default);
            }
        }

        [Config(typeof(MyConfig))]
        public static class  SerialSort
        {
            // кол-во элементов в массиве
            public static int elemsCount = 100;

            // число итераций = степени числа блоков
            public static int N = 3;

            public static int[] unsortedArray = new int[elemsCount];

            // число блоков должно быть степенью 2
            public static int blocksCount = (int) Math.Pow(2, N);
            public static int threadsCount = blocksCount / 2;
            public static List<int>[] blocks = new List<int>[blocksCount];


            // заполняем блоки
            [Benchmark]
            public static void FillBlocks()
            {
                // инициализируем блоки
                for (int i = 0; i < blocksCount; i++)
                {
                    blocks[i] = new List<int>();
                }

                for (int i = 0; i < blocks.Length; i++)
                {
                    int firstIndex = i * (elemsCount / blocksCount);
                    // если число элементов не кратно числу блоков, в последнем блоке будет больше элементов
                    int lastIndex = (i + 1) * (elemsCount / blocksCount) +
                                    (i == blocksCount - 1 ? elemsCount % blocksCount : 0);
                    for (int j = firstIndex; j < lastIndex; j++)
                    {
                        blocks[i].Add(unsortedArray[j]);
                    }
                }
            }

            [Benchmark]
            public static void CompareBlocks(int firstBlockNum, int secondBlockNum, int pivot)
            {
                // вариант с 2 циклами
                int secondBlock = blocks[secondBlockNum].Count;
                int i = 0;
                while (i < blocks[firstBlockNum].Count)
                {
                    if (blocks[firstBlockNum][i] >= pivot)
                    {
                        blocks[secondBlockNum].Add(blocks[firstBlockNum][i]);
                        blocks[firstBlockNum].Remove(blocks[firstBlockNum][i]);
                        i--;
                    }

                    i++;
                }

                i = 0;
                while (i < secondBlock)
                {
                    if (blocks[secondBlockNum][i] < pivot)
                    {
                        blocks[firstBlockNum].Add(blocks[secondBlockNum][i]);
                        blocks[secondBlockNum].Remove(blocks[secondBlockNum][i]);
                        i--;
                        secondBlock--;
                    }

                    i++;
                }
            }

            [Benchmark]
            public static void QuickBucketSort()
            {
                FillBlocks();

                for (int i = 0; i < N; i++)
                {
                    int step = (int) Math.Pow(2, N - 1 - i);
                    int first = 0;
                    int second = step;
                    int x = 0;
                    int pivot = 0;
                    while (first < blocksCount - step && second < blocksCount)
                    {
                        if (blocks[first].Count != 0)
                        {
                            // с вычислением среднего значения заполнение блоков более равномерное
                            pivot = (int) blocks[first].Average();
                            Console.WriteLine("pivot {0} i = {1}", pivot, Math.Pow(2, i));
                        }

                        while (x < step)
                        {
                            Console.WriteLine("сравнение блока {0} и {1}", first, second);
                            CompareBlocks(first, second, pivot);
                            x++;
                            first++;
                            second++;
                        }

                        x = 0;
                        first += step;
                        second += step;
                    }
                }
                
                for (int i = 0; i < blocks.Length; i++)
                {
                    blocks[i].Sort();
                }

                int[] sortedArray = blocks.SelectMany(x => x).ToArray();
                Console.WriteLine(sortedArray.Length);
                foreach (var i in sortedArray)
                {
                    Console.Write(i + " ");
                }
            }

            public static void FillArray()
            {
                Random rand = new Random();
                for (int i = 0; i < unsortedArray.Length; i++)
                {
                    unsortedArray[i] = rand.Next(0, 100);
                }
            }
        }


        static void Main(string[] args)
        {
            
            SerialSort.FillArray();
            SerialSort.QuickBucketSort();
           
        }
    }
}