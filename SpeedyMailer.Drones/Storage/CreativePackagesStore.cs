using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class CreativePackagesStore : RecordManager<CreativePackage>
	{
		public CreativePackagesStore(DroneSettings droneSettings):base(droneSettings.StoreHostname)
		{
			
		}
		public CreativePackage GetPackageForInterval(int interval)
		{
			return Find(Query.EQ(PropertyName(x => x.Interval), interval)).FirstOrDefault();
		}

		public bool AreThereAnyPackages()
		{
			return Count() > 0;
		}
	}
}
