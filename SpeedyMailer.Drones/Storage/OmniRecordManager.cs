using System.Collections.Generic;
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
	}
}