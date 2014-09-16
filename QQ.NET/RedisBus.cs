using QQ.NET.Redis;
using QQ.NET.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.NET
{
  public class DefaultChannelMapper
  {
    public string GetInputChannel<T>(T message)
    {
      return "qq:queue:input:" + typeof(T).Name;
    }
  }

  public class QueueWorker
  {
    private ConnectionMultiplexer _connectionMultiplexer;

    public QueueWorker(ConnectionMultiplexer connectionMultiplexer, string queue)
    {
      this.Queue = queue;
      this.Id = Guid.NewGuid().ToString("N");

      this.WorkQueue = "qq:queue:work:" + Id;
      _connectionMultiplexer = connectionMultiplexer;
    }

    public string Id { get; private set; }
    public QueueWorker Next { get; set; }
    public int Jobs { get; private set; }
    public string Queue { get; private set; }
    public string WorkQueue { get; private set; }

    internal void DoWork()
    {

    }
  }

  public class RedisBus : IBus
  {
    private string _connection;
    private ConnectionMultiplexer _connectionMultiplexer;
    private DefaultChannelMapper _channelMapper;

    private const int _defaultWorkersPerQueue = 8;

    public RedisBus(string connection)
    {
      _connection = connection;

      _connectionMultiplexer = ConnectionMultiplexer.Connect(_connection);
      _channelMapper = new DefaultChannelMapper();
    }

    public void Send<T>(T message)
    {
      var db = _connectionMultiplexer.GetDatabase();

      var transportMessage = new RedisTransportMessage
      {
        Id = Guid.NewGuid().ToString("N"),
        Body = message,
      };

      var serialized = Serializer.Serialize(transportMessage);

      db.HashSet("qq:values:" + typeof(T).Name, transportMessage.Id, serialized);

      //db.StringSet("qq:values:" + typeof(T).Name + ":" + transportMessage.Id, serialized);

      db.ListLeftPush(_channelMapper.GetInputChannel(message), transportMessage.Id.ToString());

    }

    private ConcurrentDictionary<string, QueueWorker> _workerPool = new ConcurrentDictionary<string, QueueWorker>();

    private QueueWorker EnsureWorkerPool(string queue)
    {
      QueueWorker worker;

      if (!_workerPool.TryGetValue(queue, out worker))
      {
        lock (_workerPool)
        {
          if (!_workerPool.TryGetValue(queue, out worker))
          {
            worker = new QueueWorker(queue);

            QueueWorker parent = worker;

            for (var i = 0; i < _defaultWorkersPerQueue; i++)
            {
              parent.Next = new QueueWorker(queue);
              parent = parent.Next;
            }

            parent.Next = worker;

            worker = _workerPool.GetOrAdd(queue, worker);
          }
        }
      }

      return worker;
    }

    public void StartListening()
    {
      var db = _connectionMultiplexer.GetDatabase();

      var subscriber = _connectionMultiplexer.GetSubscriber();

      subscriber.Subscribe("__keyspace@0__:qq:queue:input:*", (channel, command) =>
      {
        if (command == "lpush")
        {
          string channelString = channel;

          var inputQueue = channelString.Replace("__keyspace@0__:", "");

          var worker = EnsureWorkerPool(inputQueue);

          AssignWork(worker);
        }
      });
    }

    private void AssignWork(QueueWorker worker)
    {
      var root = worker;

      var selectedWorker = worker;

      var db = _connectionMultiplexer.GetDatabase();

      string value = db.ListRightPopLeftPush(selectedWorker.Queue, selectedWorker.WorkQueue);

      if (value != null) // moved message to worker queue
      {
        worker.DoWork();
      }
    }
  }
}
