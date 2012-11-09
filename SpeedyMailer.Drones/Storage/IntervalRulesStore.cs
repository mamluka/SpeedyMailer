using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Mongol;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class IntervalRulesStore : RecordManager<IntervalRule>
	{
		public IntervalRulesStore(DroneSettings droneSettings)
			: base(droneSettings.StoreHostname, null)
		{ }

		public void RemoveAll()
		{
			Collection.RemoveAll();
		}

		public IList<IntervalRule> GetAll()
		{
			return Collection.FindAllAs<IntervalRule>().ToList();
		}
	}
}