using System;
using System.Threading.Tasks.Dataflow;

namespace DataFlowTest
{
    public class TaskJoiner
    {
        public void Exec ()
        {
            var broadCastBlock = new BroadcastBlock<int>(num =>
             {
                 return num * 2;
             });

            var getNumber = new TransformBlock<int,int>(num =>
            {
                return num - 15;
            });

            var getText = new TransformBlock<int,string>(num =>
            {
                return $"this is a number {num}";
            });

            var joinBlock = new JoinBlock<int,string>();

            var actionBock = new ActionBlock<Tuple<int,string>>(t =>
            {
                Console.WriteLine(t.Item2 + " ,extra number is:" + t.Item1);
            });

            var options=new DataflowLinkOptions { PropagateCompletion = true };
            broadCastBlock.LinkTo(getNumber,options);
            broadCastBlock.LinkTo(getText,options);
            getNumber.LinkTo(joinBlock.Target1);
            getText.LinkTo(joinBlock.Target2);
            joinBlock.LinkTo(actionBock,options);

            broadCastBlock.Post(1);

            broadCastBlock.Complete();

            actionBock.Completion.Wait();

        }
    }
}
