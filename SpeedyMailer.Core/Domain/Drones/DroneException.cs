using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class DroneException
	{
		public string Message { get; set; }
		public string Exception { get; set; }
		public string Time { get; set; }
		public string Component { get; set; }
	}
}
