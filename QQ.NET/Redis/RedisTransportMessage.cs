using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.NET.Redis
{

  public class RedisTransportMessage
  {
    [JsonProperty(TypeNameHandling = TypeNameHandling.None, ItemTypeNameHandling = TypeNameHandling.Objects)]
    public Dictionary<string, object> Headers { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public object Body { get; set; }

    public string Id { get; set; }
  }
}
