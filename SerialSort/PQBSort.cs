using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;


namespace SerialSort
{
    public static partial class QBucketSort
    {
        // заполняем блоки
        private static void FillBlocksParallel(object threadIndex)
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
        private static void CompareBlocksParallel(object data)
        {
            int[] tmp = (int[]) data;
            int firstBlock = (int) tmp[0];
            int secondBlock = (int) tmp[1];
            int pivot = (int) tmp[2];
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
        
        public static void ParallelQBucketSort(int[] array)
        {
            unsortedArray = array;
            elemsCount = array.Length;
            // инициализируем блоки
            for (int i = 0; i < blocksCount; i++)
            {
                blocks[i] = new List<int>();
                blocks[i].Capacity = array.Length / blocksCount;
            }

            // параллельное заполнение блоков
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(FillBlocksParallel);
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start(i);
            }

            foreach (var t in threads)
            {
                t.Join();
            }

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
                        // с вычислением среднего значения распределение элементов по блокам более равномерное
                        pivot = (int) blocks[first].Average();
                    }

                    while (x < step)
                    {
                        threads[ind] = new Thread(CompareBlocksParallel);
                        threads[ind].Start(new int[] {first, second, pivot});
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

            // локальная сортировка
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(SortBlocksParallel);
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start(i);
            }

            foreach (var t in threads)
            {
                t.Join();
            }

            /*// итоговый массив
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(AssembleArray);
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start(i);
            }

            foreach (var t in threads)
            {
                t.Join();
            }*/
            /*for (int i = 0; i < blocksCount; i++)
            {
                blocks[i].Sort();
            }*/

            AssembleArray();
        }

        private static void SortBlocksParallel(object threadIndex)
        {
            int ind = (int) threadIndex * 2;
            blocks[ind].Sort();
            blocks[ind + 1].Sort();
        }

        /*private static void AssembleArray()
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
        }*/
        /*private static string[] GetRowFromArray(double[,] array, int column)
        {
            int dim = array.GetUpperBound(0) + 1;
            string[] row = new string[dim];

            //for (int i = 0; i < dim; i++)
            Parallel.For(0, dim, i => { row[i] = array[i, column].ToString(); });

            return row;
        }

        private static void AssembleArray(object threadIndex)
        {
            int index = (int) threadIndex * 2;


            /*int[] mergedArray = new int[blocks[index].Count + blocks[index + 1].Count];
            Array.Copy(blocks[index].ToArray(), mergedArray, blocks[index].Count);
            Array.Copy(blocks[index+1].ToArray(), 0, mergedArray, blocks[index].Count,
                blocks[index+1].Count);
            int i1 = 0;
            for (int i = blocks[index].Count * index; i < (index + 1) * blocks.Length; i++)
            {
                unsortedArray[i] = mergedArray[i1++];
            }#1#
        }*/
    }
}