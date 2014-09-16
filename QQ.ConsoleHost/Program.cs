using Newtonsoft.Json;
using QQ.NET;
using QQ.NET.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.ConsoleHost
{
  class Program
  {
    [Serializable]
    class Test
    {
      [Newtonsoft.Json.JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
      public object value { get; set; }
    }

    [Serializable]

    class Test2
    {
      public string Lol;
    }

    static void Main(string[] args)
    {
      var obj = new RedisTransportMessage()
      {
        Headers = new Dictionary<string, object>
        {
          { "trololol", "lololol" },
          { "derpderp ", new Test2() { Lol = "x" } }
        },
        Body = new Test2
        {
          Lol = "dfdsf"
        }
      };

      var serialized = JsonConvert.SerializeObject(obj);

      var x = JsonConvert.DeserializeObject<RedisTransportMessage>(serialized);

      var bus = new RedisBus("localhost");

      bus.Send(new Test2
      {
        Lol = "herpderp"
      });

      Console.ReadLine();
    }
  }
}
