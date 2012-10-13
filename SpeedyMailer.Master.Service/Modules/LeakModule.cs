using Nancy;

namespace SpeedyMailer.Master.Service.Modules
{
	public class LeakModule : NancyModule
	{
		public LeakModule():base("/admin")
		{
			Get["/leak"] = call => "OK";
		}
	}
}
