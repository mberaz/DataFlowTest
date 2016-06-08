using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlowTest
{
    public class WordLister
    {
        private string downLoadStringFunc (string uri)
        {
            Console.WriteLine("Downloading '{0}'...",uri);
            return new WebClient().DownloadString(uri);
        }

        private string[] createWordListFunc (string text)
        {
            Console.WriteLine("Creating word list...");

            // Remove common punctuation by replacing all non-letter characters 
            // with a space character to.
            char[] tokens = text.ToArray();
            for(int i = 0; i < tokens.Length; i++)
            {
                if(!char.IsLetter(tokens[i]))
                    tokens[i] = ' ';
            }
            text = new string(tokens);

            // Separate the text into an array of words.
            return text.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
        }

        private string[] filterWorldListFunc (string[] words)
        {
            Console.WriteLine("Filtering word list...");

            return words.Where(word => word.Length > 3).OrderBy(word => word).Distinct().ToArray();
        }


        private void printReversedWordsFunc (string reversedWord)
        {
            Console.WriteLine("Found reversed words {0}/{1}",reversedWord,new string(reversedWord.Reverse().ToArray()));
        }

        public void ListWords ()
        {
            // Create the members of the pipeline.
            Console.WriteLine("start");
            // Downloads the requested resource as a string.

            var downloadString = new TransformBlock<string,string>(uri => downLoadStringFunc(uri));

            // Separates the specified text into an array of words.
            var createWordList = new TransformBlock<string,string[]>(text => createWordListFunc(text));

            // Removes short words, orders the resulting words alphabetically, 
            // and then remove duplicates.
            var filterWordList = new TransformBlock<string[],string[]>(words => filterWorldListFunc(words));

            // Finds all words in the specified collection whose reverse also 
            // exists in the collection.
            var findReversedWords = new TransformManyBlock<string[],string>(words =>
            {
                Console.WriteLine("Finding reversed words...");

                // Holds reversed words.
                var reversedWords = new ConcurrentQueue<string>();

                // Add each word in the original collection to the result whose 
                // reversed word also exists in the collection.
                Parallel.ForEach(words,word =>
                {
                    // Reverse the work.
                    string reverse = new string(word.Reverse().ToArray());

                    // Enqueue the word if the reversed version also exists
                    // in the collection.
                    if(Array.BinarySearch<string>(words,reverse) >= 0 &&
                            word != reverse)
                    {
                        reversedWords.Enqueue(word);
                    }
                });
                return reversedWords;
            });

            // Prints the provided reversed words to the console.    
            var printReversedWords = new ActionBlock<string>(reversedWord => printReversedWordsFunc(reversedWord));

            downloadString.LinkTo(createWordList);
            createWordList.LinkTo(filterWordList);
            filterWordList.LinkTo(findReversedWords);
            findReversedWords.LinkTo(printReversedWords);


            downloadString.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock)createWordList).Fault(t.Exception);
                else createWordList.Complete();
            });
            createWordList.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock)filterWordList).Fault(t.Exception);
                else filterWordList.Complete();
            });
            filterWordList.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock)findReversedWords).Fault(t.Exception);
                else findReversedWords.Complete();
            });
            findReversedWords.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock)printReversedWords).Fault(t.Exception);
                else printReversedWords.Complete();
            });

            downloadString.Post("http://www.gutenberg.org/files/6130/6130-0.txt");
            downloadString.Complete();

            printReversedWords.Completion.Wait();

            Console.Read();
        }
    }
}
