using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Platform.Triggers;

namespace WebhookTrigger
{
    //Each trigger may declare a type that sub-classes TriggerArgs 
    //that corresponds to the “args” item in Trigger Scope activity. If no extra info 
    //needs to be passed along, TriggerArgs can be used directly 

    public class WebhookTriggerArgs : TriggerArgs
    {
        public string JsonBody { get; }
        public string Path { get; }
        public int Port { get; }


        public WebhookTriggerArgs(string body, string path, int port)
        {
            this.JsonBody = body;
            this.Path = path;
            this.Port = port;
        }
    }
}
