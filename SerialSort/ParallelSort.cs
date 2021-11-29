using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    [SimpleJob(RunStrategy.Throughput, targetCount:10)]
    public class ParallelSort
        {
            // кол-во элементов в массиве
            [Params(100, 10000)] 
            public static int elemsCount = 100;

            // число итераций = степени числа блоков
            [Params(3, 4)] 
            public static int N = 3;

            public int[] unsortedArray;

            // число блоков должно быть степенью 2
            public static int blocksCount = (int) Math.Pow(2, N);
            public static int threadsCount = blocksCount / 2;
            private Thread[] threads = new Thread[threadsCount];
            public static List<int>[] blocks = new List<int>[blocksCount];
            private int blocksStep = blocksCount / threadsCount;
            
            int elemsStep = elemsCount / threadsCount;
            int lastIndex = elemsCount % threadsCount;
            
            
            
            // заполняем блоки
            public void FillBlocks(object threadIndex)
            {
                int index = (int) threadIndex;
                int startIndex = index * blocksStep;
                int finishIndex = (index + 1) * blocksStep;

                for (int i = startIndex; i < finishIndex; i++)
                {
                    int firstElemIndex = i * (elemsCount / blocksCount);
                    // если число элементов не кратно числу блоков, в последнем блоке будет больше элементов
                    int lastElemIndex = (i + 1) * (elemsCount / blocksCount) +
                                    (i == blocksCount - 1 ? elemsCount % blocksCount : 0);
                    for (int j = firstElemIndex; j < lastElemIndex; j++)
                    {
                        blocks[i].Add(unsortedArray[j]);
                    }
                }
            }
            
            public void CompareBlocks(object data)
            {
                int[] tmp = (int[]) data;
                int firstBlockNum = (int) tmp[0];
                int secondBlockNum = (int) tmp[1];
                int pivot = (int) tmp[2];
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
                unsortedArray = new int[elemsCount];
                // инициализируем блоки
                for (int i = 0; i < blocksCount; i++)
                {
                    blocks[i] = new List<int>();
                }
                
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(FillArray);
                    threads[i].Start(i);
                }

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Join();
                    threads[i] = new Thread(FillBlocks);
                    threads[i].Start(i);
                }

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Join();
                }
                //FillArray();
               // FillBlocks();
                for (int i = 0; i < N; i++)
                {
                    int step = (int) Math.Pow(2, N - 1 - i);
                    int first = 0;
                    int second = step;
                    int x = 0;
                    int pivot = 0;
                    int ind = 0;
                    while (first < blocksCount - step && second < blocksCount)
                    {
                        if (blocks[first].Count != 0)
                        {
                            // с вычислением среднего значения заполнение блоков более равномерное
                            pivot = (int) blocks[first].Average();
                        }
                        while (x < step)
                        {
                            threads[ind] = new Thread(CompareBlocks);
                            threads[ind].Start(new int[]{first,second,pivot});
                            ind++;
                            //Console.WriteLine("сравнение блока {0} и {1}", first, second);
                          //  CompareBlocks(first, second, pivot);
                            x++;
                            first++;
                            second++;
                        }

                        x = 0;
                        first += step;
                        second += step;
                    }

                    for (int i1 = 0; i1 < threads.Length; i1++)
                    {
                        threads[i1].Join();
                    }
                }
                for (int i = 0; i < blocks.Length; i++)
                {
                    blocks[i].Sort();
                }

                int[] result = blocks.SelectMany(x => x).ToArray();
                foreach (var i in result)
                {
                    Console.Write(i+" ");
                }
            }
            public void FillArray(object threadIndex)
            {
                int index = (int) threadIndex;
                int startIndex = index * elemsStep;
                int finishIndex = (index + 1) * elemsStep + (index == threadsCount - 1 ? lastIndex : 0);
                
                Random rand = new Random();
                for (int i = startIndex; i < finishIndex; i++)
                {
                    unsortedArray[i] = rand.Next(0, 100);
                }
            }
        }


}