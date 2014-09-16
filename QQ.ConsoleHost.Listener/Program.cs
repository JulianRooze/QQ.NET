using QQ.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.ConsoleHost.Listener
{
  class Program
  {
    static void Main(string[] args)
    {

      var bus = new RedisBus("localhost");

      bus.StartListening();

      Console.ReadLine();

    }
  }
}
