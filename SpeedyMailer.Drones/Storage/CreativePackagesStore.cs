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
        private readonly CreativeFragmentSettings _creativeFragmentSettings;

        public CreativePackagesStore(DroneSettings droneSettings, CreativeFragmentSettings creativeFragmentSettings)
            : base(droneSettings.StoreHostname)
        {
            _creativeFragmentSettings = creativeFragmentSettings;
        }

        public CreativePackage GetPackageForGroup(string group)
        {
            return Find(Query.EQ(PropertyName(x => x.Group), group)
                             .And(Query.EQ(PropertyName(x => x.Processed), false))).FirstOrDefault();

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
            return Collection.Find(Query.In(PropertyName(x => x.To), emails.Select(x => (BsonString)x))).ToList();
        }

        public IList<CreativePackage> GetUnprocessedDefaultGroupPackages()
        {
            return Collection.Find(Query.EQ(PropertyName(x => x.Group), _creativeFragmentSettings.DefaultGroup).And(Query.EQ(PropertyName(x => x.Processed), false))).ToList();
        }
    }
}
