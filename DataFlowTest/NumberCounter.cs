using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace DataFlowTest
{
    public class NumberCounter
    {
        private List<int> CreateNumbersFunc (int count)
        {
            return Enumerable.Range(0,count).ToList();
        }

        private void PrintNumberFunc (int num)
        {
            Console.WriteLine($"A number : {num}");
        }

        public void Exec ()
        {
            Console.WriteLine("start");
            // Downloads the requested resource as a string.

            var createNumbers = new TransformManyBlock<int,int>(count => CreateNumbersFunc(count));

            var printNumber = new ActionBlock<int>(number => PrintNumberFunc(number));

            createNumbers.LinkTo(printNumber);

            createNumbers.Completion.ContinueWith(t => printNumber.Complete());

            createNumbers.Post(10);
            createNumbers.Complete();

            printNumber.Completion.Wait();

            Console.Read();
        }
    }
}
