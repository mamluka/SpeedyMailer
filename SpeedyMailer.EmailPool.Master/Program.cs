using System;
using Nancy.Hosting.Self;

namespace SpeedyMailer.EmailPool.Master
{
    class Program
    {
        static void Main(string[] args)
        {
            var nancyHost = new NancyHost(new Uri("http://localhost:12345/"));
            nancyHost.Start();

            
            
            Console.WriteLine("Nancy now listening - navigating to http://localhost:8888/nancy/. Press enter to stop");
            Console.ReadKey();
            
            nancyHost.Stop();
            Console.WriteLine("Stopped. Good bye!");
        }
    }
}
