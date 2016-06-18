using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlowTest
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Joiner
    {
        public UserModel Crete(int id, string name) 
        {
            var jb =new JoinBlock<int,string>();
            jb.Target1.Post(id);
            jb.Target2.Post(name);


            var result = jb.Receive();
            var res = new UserModel {
                Id=result.Item1,
                Name=result.Item2
            };

            return res;
        }
    }
}
