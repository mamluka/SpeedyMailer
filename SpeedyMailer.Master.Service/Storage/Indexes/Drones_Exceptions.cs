using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Drones_Exceptions : AbstractIndexCreationTask<Drone, Drones_Exceptions.ReduceResult>
	{
		public class ReduceResult
		{
			public string Group { get; set; }
			public List<string> Exceptions { get; set; }
		}

		public Drones_Exceptions()
		{
			Map = drones => drones.Select(x => new { Group = "All", Exceptions = x.Exceptions.Select(m => m.Time + " " + x.Domain + " " + m.Component + " " + m.Message + " " + m.Exception).ToList() });

			Reduce = result => result
								   .GroupBy(x => x.Group)
								   .Select(x => new { Group = x.Key, Exceptions = x.SelectMany(s => s.Exceptions) });
		}
	}
}
