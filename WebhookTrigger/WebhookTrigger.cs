using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.Threading;
using UiPath.Platform.Triggers;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

namespace WebhookTrigger
{

    public class WebhookTrigger : TriggerBase<WebhookTriggerArgs>
    {

        //it is recommended to use Variable to store fields in order for  

        //activities like Parallel For Each to work correctly 
        //private readonly Variable<Timer> _timer = new Variable<Timer>();
        [Browsable(false)]
        public InArgument<string> Method { get; set; } // unused 
        [RequiredArgument]
        public InArgument<string> Path { get; set; } = "/trigger";
        [RequiredArgument]
        public InArgument<Int32> Port { get; set; } = 8000;

        private WebRequestHandler handler;
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            //metadata.AddImplementationVariable(_timer);
            base.CacheMetadata(metadata);
        }


        //in this method you can subscribe to events. It is called when the trigger starts execution 
        protected override void StartMonitor(NativeActivityContext context, Action<WebhookTriggerArgs> sendTrigger)
        {
#if DEBUG
            Console.WriteLine("Starting Monitoring ...");
#endif
            var path = Path.Get(context);
            var port = Port.Get(context);

            //복수개 ip가 있는 경우 고려해야 함. 
            string localIP = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            //var url = string.Format("http://{0}:{1}/", localIP, port);

            handler = new WebRequestHandler(localIP, port, path);
            handler.Start( sendTrigger);
#if DEBUG
            Console.WriteLine("Leaving StartMonitoring");
#endif
        }



        //this is used for cleanup. It is called when the trigger is Cancelled or Aborted 
        protected override void StopMonitor(ActivityContext context)
        {
            handler.Stop();
        }

    }



}
