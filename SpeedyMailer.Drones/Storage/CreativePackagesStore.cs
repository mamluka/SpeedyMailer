using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class CreativePackagesStore : RecordManager<CreativePackage>
	{
		public CreativePackagesStore(DroneSettings droneSettings)
			: base(droneSettings.StoreHostname)
		{

		}
		public CreativePackage GetPackageForGroup(string group)
		{
			return AsQueryable.FirstOrDefault(x => x.Group == @group && x.Processed == false);
		}

		public IList<string> GetPackageGroups()
		{
			return AsQueryable
				.GroupBy(x => x.Group)
				.Select(x => x.Key)
				.ToList();
		}

		public bool AreThereAnyNonProcessedPackages()
		{
			return Count() > 0;
		}

		public IList<CreativePackage> GetAll()
		{
			return Collection
				.Find(Query.EQ(PropertyName(x => x.Processed), false))
				.ToList();
		}

		public List<CreativePackage> GetByEmail(IEnumerable<string> emails)
		{
			return AsQueryable.Where(x => emails.Contains(x.To)).ToList();
		}
	}
}
