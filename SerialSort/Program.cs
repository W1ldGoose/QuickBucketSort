using BenchmarkDotNet.Running;

namespace SerialSort
{
    public class Program
    {
        static void Main(string[] args)
        {
           // ParallelSort parallelSort = new ParallelSort();
           // parallelSort.QuickBucketSort();
            var summary = BenchmarkRunner.Run<ParallelSort>();
            // var summary = BenchmarkRunner.Run<SerialSort>();
        }
    }
}