using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class CreativePackagesStore : RecordManager<CreativePackage>,ICycleSocket
	{
		public CreativePackagesStore(DroneSettings droneSettings)
			: base(droneSettings.StoreHostname)
		{

		}
		public CreativePackage GetPackageForGroup(string group)
		{
			return Find(Query.EQ(PropertyName(x => x.Group), group)).FirstOrDefault();
		}

		public bool AreThereAnyPackages()
		{
			return Count() > 0;
		}

		public void CycleSocket()
		{
			Count();
		}
	}
}
