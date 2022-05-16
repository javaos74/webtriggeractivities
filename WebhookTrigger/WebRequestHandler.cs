using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebhookTrigger
{
    public class WebRequestHandler 
    {
        private HttpListener listener;
        private static int requestCount = 0;

        private bool runServer = true;
        private string path;
        private int port;

        public WebRequestHandler( string ip, int port, string path)
        {
            runServer = true;
            this.path = path;
            this.port = port;
            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, port));
            listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
            listener.Start();
        }
        public void Stop()
        {
#if DEBUG
            Console.WriteLine("WebRequestHandler stopped");
#endif
            runServer = false;
            listener.Stop();
        }
        public async void Start(Action<WebhookTriggerArgs> sendTrigger)
        {
            // While a user hasn't visited the `shutdown` url, keep on handling requests
#if DEBUG
            Console.WriteLine("WebRequestHandler started");
#endif
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                if (!runServer)
                    return;
                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;


#if DEBUG
                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine(req.Url.AbsolutePath);
                Console.WriteLine();

#endif

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == this.path))
                {
                    Int64 n = req.ContentLength64;
                    if (n > 0)
                    {
                        byte[] buf = new byte[n];
                        await req.InputStream.ReadAsync(buf, 0, (Int32)n);
                        sendTrigger(new WebhookTriggerArgs(Encoding.UTF8.GetString(buf), req.Url.AbsoluteUri, this.port));
                    } 
                    else
                    {
                        sendTrigger(new WebhookTriggerArgs("", req.Url.AbsoluteUri, this.port));
                    }
                    resp.AppendHeader("Access-Control-Allow-Origin", "*");
                }
                if( (req.HttpMethod == "OPTIONS") && (req.Url.AbsolutePath.StartsWith( this.path)))
                {
                    resp.AppendHeader("Access-Control-Allow-Origin", "*");
#if DEBUG
                    Console.WriteLine("CORS allowed");
#endif
                }
                if( (req.HttpMethod == "GET") && (req.Url.AbsolutePath.StartsWith(this.path)))
                {
#if DEBUG
                    Console.WriteLine("QueryString: " + req.Url.PathAndQuery);
#endif
                    sendTrigger(new WebhookTriggerArgs(req.Url.PathAndQuery, req.Url.AbsoluteUri, this.port));
                }


                resp.Close();
            }
        }

    }
}
