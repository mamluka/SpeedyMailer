using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Builders;
using Mongol;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class GenericRecordManager<T> : RecordManager<T>, ICycleSocket where T : class
	{
		public GenericRecordManager(string connectionString, string collectionName = null)
			: base(connectionString, collectionName)
		{ }

		public IList<T> FindAll()
		{
			return Collection.FindAllAs<T>().ToList();
		}

		public bool Exists(int count = -1)
		{
			return Count() >= count;
		}

		public void CycleSocket()
		{
			Count();
		}
	}
}