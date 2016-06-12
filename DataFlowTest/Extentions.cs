﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlowTest
{
    public static class Extentions
    {
        public static void CompleteWhenAll (this IDataflowBlock target,params IDataflowBlock[] sources)
        {
            if(target == null) return;
            if(sources.Length == 0)
            {
                target.Complete();
                return;
            }

            Task.Factory.ContinueWhenAll(
                sources.Select(b => b.Completion).ToArray(),
                tasks => {
                    var exceptions = (from t in tasks where t.IsFaulted select t.Exception).ToList();
                    if(exceptions.Count != 0)
                    {
                        target.Fault(new AggregateException(exceptions));
                    }
                    else
                    {
                        target.Complete();
                    }
                }
            );
        }
    }
}
