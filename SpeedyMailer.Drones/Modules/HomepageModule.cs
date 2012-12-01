using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace SpeedyMailer.Drones.Modules
{
	public class HomepageModule:NancyModule
	{
		public HomepageModule()
		{
			Get["/"] = x => "Hello, have a wonderful day!";
		}
	}
}
