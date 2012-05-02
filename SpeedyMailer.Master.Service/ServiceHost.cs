using System;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using SpeedyMailer.Master.Service.Bootstrapper;

namespace SpeedyMailer.Master.Service
{
    public class ServiceHost
    {
    	private static NancyHost _nancyHost;

    	public static void Main(string[] args)
    	{
    		_nancyHost = new NancyHost(new MyNinjectBootstrapper(),new Uri("http://localhost:2589/"));
    		StartNancy();
    	}

		public static void Main(NinjectNancyBootstrapper ninjectNancyBootstrapper)
		{
			_nancyHost = new NancyHost(ninjectNancyBootstrapper, new Uri("http://localhost:2589/"));
			StartNancy();
		}


    	private static void StartNancy()
    	{
    		_nancyHost.Start();

    		Console.WriteLine("Nancy now listening - navigating to http://localhost:2589. Press enter to stop");
    		Console.ReadKey();

    		_nancyHost.Stop();
    		Console.WriteLine("Stopped. Good bye!");
    	}
    }
}