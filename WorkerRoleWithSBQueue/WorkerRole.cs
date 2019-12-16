using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Numerics;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRoleWithSBQueue
{
    using Newtonsoft.Json;

    public class TaskModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "input")]
        public string Input { get; set; }
    }

    public class WorkerRole : RoleEntryPoint
    {
        QueueClient TaskQueueClient;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            TaskQueueClient.OnMessage((received) =>
                {
                    try
                    {
                        Stream stream = received.GetBody<Stream>();
                        StreamReader reader = new StreamReader(stream);
                        string s = reader.ReadToEnd();
                        TaskModel task = JsonConvert.DeserializeObject<TaskModel>(s);
                        BigInteger result = Calculate(Int32.Parse(task.Input));

                        
                    }
                    catch
                    {
                    }
                });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            TaskQueueClient = QueueClient.CreateFromConnectionString(connectionString, "uni-task-queue");
            return base.OnStart();
        }

        public override void OnStop()
        {
            TaskQueueClient.Close();
            CompletedEvent.Set();
            base.OnStop();
        }

        private static BigInteger Calculate(int n)
        {
            BigInteger result = new BigInteger(1);

            for (int i = 1; i <= n; i++)
            {
                result = BigInteger.Multiply(result, i);
            }

            return result;
        }
    }
}
