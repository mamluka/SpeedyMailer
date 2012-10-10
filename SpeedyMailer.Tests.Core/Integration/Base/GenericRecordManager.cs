using System.Collections.Generic;
using System.Linq;
using Mongol;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class GenericRecordManager<T> : RecordManager<T> where T : class
	{
		public GenericRecordManager(string connectionString) : base(connectionString)
		{}

		public IList<T> FindAll()
		{
			return collection.FindAllAs<T>().ToList();
		}
	}
}