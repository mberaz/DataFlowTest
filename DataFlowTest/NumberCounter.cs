﻿using System;
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
            var printSecondFormat = new ActionBlock<int>(number =>
            {
                if(number == 4 || number == 5)
                {
                    throw new Exception("new error");
                }

                Console.WriteLine($" Second format {number}");
            });

            var errorReportBlock = new ActionBlock<int>(number => { Console.WriteLine($" Error on: {number}"); });

            //1.work in parralel
            //var printNumber = new ActionBlock<int>(number => PrintNumberFunc(number),new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 });

            //2.filter mesages
            //createNumbers.LinkTo(printNumber,n=>n%2==0 );
            //createNumbers.LinkTo(printSecondFormat,n => n % 2 != 0);


            //3.work in 2 blocks
            var broadcast = new BroadcastBlock<int>(null);

            createNumbers.LinkTo(broadcast);
            broadcast.LinkTo(printSecondFormat);
            broadcast.LinkTo(printNumber);

            createNumbers.Completion.ContinueWith(t => broadcast.Complete());
            broadcast.Completion.ContinueWith(t => printNumber.Complete());
            //createNumbers.Completion.ContinueWith(t => printSecondFormat.Complete());
            createNumbers.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    ((IDataflowBlock)printSecondFormat).Fault(t.Exception);
                }
                else printSecondFormat.Complete();
            });

            createNumbers.Post(10);

            try
            {
                createNumbers.Complete();

                printSecondFormat.Completion.Wait();
                printNumber.Completion.Wait();
            }
            catch(AggregateException ex)
            {
                Console.WriteLine(ex.Flatten().Message);
            }
            Console.Read();
        }
    }
}
