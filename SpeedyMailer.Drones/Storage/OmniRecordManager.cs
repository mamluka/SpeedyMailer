using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Wrappers;
using Mongol;
using SpeedyMailer.Core.Domain.Mail;
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

		public T Load<T>() where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			return specificRecordManager.GetById(typeof (T).FullName);
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

		public void EnsureIndex<T>(params Expression<Func<T, object>>[] expression) where T : class
		{
			var specificRecordManager = new RecordManager<T>(_droneSettings.StoreHostname);
			var indexKeysDocument = new IndexKeysBuilder();

			var objectExpressions = expression
				.Select(x => x.Body as MemberExpression)
				.Where(x => x != null)
				.Select(x => x.Member.Name)
				.ToArray();

			var valueExpressions = expression
				.Select(x => x.Body as UnaryExpression)
				.Where(x => x != null)
				.Select(x => x.Operand as MemberExpression)
				.Where(x => x != null)
				.Select(x => x.Member.Name)
				.ToArray();

			var keys = objectExpressions.Concat(valueExpressions).ToArray();

			indexKeysDocument.Ascending(keys);

			var indexOptionsBuilder = new IndexOptionsBuilder();
			indexOptionsBuilder.SetName(string.Format("{0}_{1}", typeof(T).Name, string.Join("_", keys)));

			specificRecordManager.Collection.EnsureIndex(indexKeysDocument, indexOptionsBuilder);
		}
	}
}