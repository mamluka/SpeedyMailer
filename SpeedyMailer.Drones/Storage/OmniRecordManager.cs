using System.Collections.Generic;
using System.Linq;
using Mongol;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Storage
{
	public class OmniRecordManager
	{
		private readonly DroneSettings _droneSettings;

		public OmniRecordManager(DroneSettings droneSettings)
		{
			_droneSettings = droneSettings;
		}

		public void BatchInsert<T>(IList<T> records) where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			specificRecordManager.BatchInsert(records);
		}

		public void UpdateOrInsert<T>(T record) where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);

			specificRecordManager.Save(record);
		}

		public T GetSingle<T>() where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			return specificRecordManager.AsQueryable.FirstOrDefault();
		}
	}
}