using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Wrappers;
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

		public IList<T> GetAll<T>() where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			return specificRecordManager.Collection.FindAllAs<T>().ToList();
		}

		public T Load<T>(string documentId) where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			return specificRecordManager.GetById(documentId);
		}

		public void DeleteConnection<T>() where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			specificRecordManager.Collection.Drop();
		}

		public void EnsureIndex<T>(params Expression<Func<T,object>>[] keys) where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			specificRecordManager.Collection.EnsureIndex(new IndexKeysDocument());
		}
	}
}