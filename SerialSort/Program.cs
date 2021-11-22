using System;
using System.Collections.Generic;
using System.Linq;

namespace SerialSort
{
    class Program
    {
        // кол-во элементов в массиве
        public static int elemsCount = 104;

        // число итераций = степени числа блоков
        public static int N = 3;

        public static int[] unsortedArray = new int[elemsCount];

        // число блоков должно быть степенью 2
        public static int blocksCount = (int) Math.Pow(2, N);
        public static int threadsCount = blocksCount / 2;
        public static int currentBlockNum = blocksCount / 2;
        public static int previousBlockNum = blocksCount;
        public static List<int>[] blocks = new List<int>[blocksCount];
        public static int[] sortedArray = new int[elemsCount];


        /*public static string ToBinary(int num)
        {
            string tmp = Convert.ToString(num, 2).PadLeft(3, '0');
            string result = new string(tmp.Reverse().ToArray());
            return result;
        }*/

        // заполняем блоки
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

        public static void CompareBlocks(int firstBlockNum, int secondBlockNum, int pivot)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < blocks[firstBlockNum].Count; i++) temp.Add(blocks[firstBlockNum][i]);
            for (int i = 0; i < blocks[secondBlockNum].Count; i++) temp.Add(blocks[secondBlockNum][i]);
            blocks[firstBlockNum].Clear();
            blocks[secondBlockNum].Clear();
            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i] > pivot)
                {
                    blocks[secondBlockNum].Add(temp[i]);
                }
                else
                {
                    blocks[firstBlockNum].Add(temp[i]);
                }
            }
        }
        static void AssembleArray()
        {
            int ind = 0;
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < blocks[i].Count; j++)
                {
                    sortedArray[ind] = blocks[i][j];
                    ind++;
                }
            }
        }
        public static void QuickBucketSort()
        {
            FillBlocks();
            
            int pivot = 0;
            while (currentBlockNum > 0)
            {
                for (int i = 0; i < blocksCount; i += previousBlockNum)
                {
                    for (int j = i; j < i + currentBlockNum; j++)
                    {
                        if (blocks[j].Count != 0)
                        {
                            pivot = blocks[j][blocks[j].Count / 2];
                            break;
                        }
                    }

                    for (int j = i; j < i + currentBlockNum; j++)
                    {
                        CompareBlocks(j,j+currentBlockNum, pivot);
                    }
                }
                previousBlockNum = currentBlockNum;
                currentBlockNum /= 2;
            }

            // N операций взаимодействия пар блоков
            for (int i = 0; i < N; i++)
            {
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < blocks[i].Count; j++)
                {
                    Console.Write(blocks[i][j] + " ");
                }

                Console.WriteLine();
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

        static void Main(string[] args)
        {
            FillArray();
            QuickBucketSort();
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Sort();
            }
            foreach (var el in sortedArray) Console.Write(el + " ");
        }
    }
}