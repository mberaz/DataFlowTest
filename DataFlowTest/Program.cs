using DataFlowTest;
using System;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace DataFlowTest
{
    internal class Program
    {
        private static void Main ()
        {
            //var wordLister = new WordLister();
            //wordLister.ListWords();

            //var peopleHandler = new PeopleHandler();
            //peopleHandler.Handle();


            var NumberCounter = new NumberCounter();
            NumberCounter.Exec();
        }
    }
}