using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;

namespace SerialSort
{
    
    [MemoryDiagnoser()]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn()]
    [RPlotExporter]
    // Usually, you shouldn't specify such characteristics like LaunchCount, WarmupCount, TargetCount, or IterationTime
    // because BenchmarkDotNet has a smart algorithm to choose these values automatically based on received measurements.
    [SimpleJob(RunStrategy.Throughput, 3)]
   public class SerialSort
        {
            // кол-во элементов в массиве
            [Params(100, 10000)] 
            public int elemsCount;

            // число итераций = степени числа блоков
            [Params(3, 4)] 
            public static int N;

            public int[] unsortedArray;

            // число блоков должно быть степенью 2
            public static int blocksCount = (int) Math.Pow(2, N);
            public static int threadsCount = blocksCount / 2;
            public static List<int>[] blocks = new List<int>[blocksCount];


            // заполняем блоки
            public void FillBlocks()
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
            
            public void CompareBlocks(int firstBlockNum, int secondBlockNum, int pivot)
            {
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
            public void QuickBucketSort()
            {
                FillArray();
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
                        }
                        while (x < step)
                        {
                            //Console.WriteLine("сравнение блока {0} и {1}", first, second);
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
            }

            public void FillArray()
            {
                unsortedArray = new int[elemsCount];
                Random rand = new Random();
                for (int i = 0; i < unsortedArray.Length; i++)
                {
                    unsortedArray[i] = rand.Next(0, 100);
                }
            }
        }

}