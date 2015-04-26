using System;

namespace EdisonSockets
{
    using Emgu.CV;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Hosting;
    using Owin;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Timers;
    using Timer = System.Timers.Timer;

    class Program
    {
        //const string ip = "localhost";
        static string ip = "192.168.1.11";
        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.

            ip = LocalIPAddress();
            string urlSockets = "http://" + ip + ":8088";
            string urlServer = string.Format("http://{0}:8090/", ip);
            var server = new WebServer.SimpleWebServer(SendResponse, urlServer);
            server.Run();

            using (WebApp.Start(urlSockets))
            {
                Console.WriteLine("SocketSrever running on {0}", urlSockets);
                Console.WriteLine("Connect with browser to " + urlServer);
                Console.ReadLine();
            }

            server.Stop();
        }

        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    var split = localIP.Split('.');
                    if (split[2] == "1")
                        break;
                }
            }
            return localIP;
        }

        private static string SendResponse(HttpListenerRequest arg)
        {
            string filecontent;
            Console.WriteLine(arg.RawUrl);
            if (arg.RawUrl == "/")
                filecontent = File.ReadAllText(@"client/index.html");
            else
            {
                filecontent = File.ReadAllText(@"client" + arg.RawUrl);
                Console.WriteLine(@"client" + arg.RawUrl);
            }
            filecontent = filecontent.Replace("#ip", ip);

            return filecontent;
        }
    }
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }


    [HubName("MyHub")]
    public class MyHub : Hub
    {
        Timer t = new Timer(100);
        private static int clientCount = 0;
        private static Capture capture = new Capture();

        public override Task OnConnected()
        {
            clientCount++;
            t.Elapsed += (o, args) => PushTimeToClient(o, args);
            t.Start();

            if (clientCount == 1)
            {
                if (capture == null)
                {
                    capture=new Capture();
                }
                capture.ImageGrabbed += capture_ImageGrabbed;
                capture.Start();
            }

            Console.WriteLine("{0} Clients connected!", clientCount);
            return base.OnConnected();
        }

        void capture_ImageGrabbed(object sender, EventArgs e)
        {
            var retrieveFrame = capture.RetrieveBgrFrame();//.Resize(320,240, Inter.Linear);
            PushFrames(ImageToByte(retrieveFrame.Bitmap));
        }
        
        public override Task OnDisconnected(bool stopCalled)
        {
            clientCount--;
            if (clientCount == 0)
                VideoStop();
            Console.WriteLine("{0} clients active",clientCount);
            return base.OnDisconnected(stopCalled);
        }

        public Task PushTimeToClient(object sender, ElapsedEventArgs e)
        {
            Clients.Caller.whatTimeIsIt(DateTime.Now.ToLongTimeString());
            return Task.FromResult(true);
        }

        public Task PushFrames(byte[] data)
        {
            var base64String = Convert.ToBase64String(data);
            Clients.Caller.frame(base64String);
            return Task.FromResult(true);
        }

        private void VideoStop()
        {
            capture.Stop();
            capture = null;
            Console.WriteLine("Video stopped!");
        }



        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Debug.Print("Disposing Hub");
                //capture = null;
                //t.Stop();
                //t.Dispose();
                //clientCount = 0;
            }
            base.Dispose(disposing);
        }
    }
}
