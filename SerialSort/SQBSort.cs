using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace SerialSort
{
    
   public static partial class QBucketSort
        {
        
            // число итераций = степени числа блоков
            public static int N = 3;
            private static int[] unsortedArray;

            private static int elemsCount;
            // число блоков должно быть степенью 2
            public static int blocksCount = (int) Math.Pow(2, N);
            public static List<int>[] blocks = new List<int>[blocksCount];
            private static int threadsCount = blocksCount / 2;
            private static Thread[] threads = new Thread[threadsCount];
        
            private static int blocksStep = blocksCount / threadsCount;


            // заполняем блоки
            private static void FillBlocksSerial()
            {
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
            
            private static void CompareBlocksSerial(int firstBlock, int secondBlock, int pivot)
            {
                int[] mergedArray = new int[blocks[firstBlock].Count + blocks[secondBlock].Count];
            
                Array.Copy(blocks[firstBlock].ToArray(), mergedArray, blocks[firstBlock].Count);
                Array.Copy(blocks[secondBlock].ToArray(), 0, mergedArray, blocks[firstBlock].Count,
                    blocks[secondBlock].Count);
                blocks[firstBlock].Clear();
                blocks[secondBlock].Clear();
                for (int i = 0; i < mergedArray.Length; i++)
                {
                    if (mergedArray[i] >= pivot)
                    {
                        blocks[secondBlock].Add(mergedArray[i]);
                    }
                    else
                    {
                        blocks[firstBlock].Add(mergedArray[i]);
                    }
                }
            }
            
            public static void SerialQBucketSort(int[] array)
            {
                unsortedArray = array;
                elemsCount = array.Length;

               // инициализируем блоки
               for (int i = 0; i < blocksCount; i++)
               {
                   blocks[i] = new List<int>();
                   blocks[i].Capacity = array.Length / blocksCount;
               }
                FillBlocksSerial();
                
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
                            CompareBlocksSerial(first, second, pivot);
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
                AssembleArray();
            }

            private static void AssembleArray()
            {
                int i1 = 0;
                for (int i = 0; i < blocksCount; i++)
                {
                    for (int j = 0; j < blocks[i].Count; j++)
                    {
                        unsortedArray[i1] = blocks[i][j];
                        i1++;
                    }
                }
            }
        }

}