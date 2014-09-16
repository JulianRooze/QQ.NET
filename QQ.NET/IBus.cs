using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.NET
{
  public interface IBus
  {
    void Send<T>(T message);
  }
}
