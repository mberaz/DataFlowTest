using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace DataFlowTest
{
    public class PeopleHandler
    {
        public void Handle ()
        {
            var inputBlock = new BufferBlock<string>();

            var transformInputBlock = new TransformBlock<string,XDocument>(s => XDocument.Parse(s));

            var processBlock = new TransformBlock<XDocument,Tuple<string,int>>(
                x =>
                {
                    var person = x.Element("person");
                    return Tuple.Create((string)person.Element("name"),(int)person.Element("age"));
                });

            var transformOutputBlock =
                new TransformBlock<Tuple<string,int>,string>(t => string.Format(CultureInfo.CurrentCulture,"{0} is {1} years old",t.Item1,t.Item2));

            var outputBlock = new ActionBlock<string>(m => Console.Out.WriteLine(m));

            using(inputBlock.LinkTo(transformInputBlock))
            using(transformInputBlock.LinkTo(processBlock))
            using(processBlock.LinkTo(transformOutputBlock))
            using(transformOutputBlock.LinkTo(outputBlock))
            {
                inputBlock.Completion.ContinueWith(t => transformInputBlock.Complete());
                transformInputBlock.Completion.ContinueWith(t => processBlock.Complete());
                processBlock.Completion.ContinueWith(t => transformOutputBlock.Complete());
                transformOutputBlock.Completion.ContinueWith(t => outputBlock.Complete());

                var records = new[]
                    {
                    "<person><name>Michael Collins</name><age>38</age></person>",
                    "<person><name>George Washington</name><age>281</age></person>",
                    "<person><name>Abraham Lincoln</name><age>204</age></person>"
                };

                foreach(var record in records)
                {
                    inputBlock.Post(record);
                }

                inputBlock.Complete();
                outputBlock.Completion.Wait();

                Console.Read();

            }
        }
    }
}
