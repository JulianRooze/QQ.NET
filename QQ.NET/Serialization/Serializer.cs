using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.NET.Serialization
{
  public static class Serializer
  {
    public static string Serialize<T>(T obj)
    {
      return JsonConvert.SerializeObject(obj, Formatting.None);
    }

    public static T Deserialize<T>(string serialized)
    {
      return JsonConvert.DeserializeObject<T>(serialized);
    }
  }
}
