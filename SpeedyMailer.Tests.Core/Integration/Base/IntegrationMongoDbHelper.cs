using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Runner;
using MongoDB.Runner.Configuration;
using Mongol;
using Ninject;
using SpeedyMailer.Core;
using SpeedyMailer.Drones;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationMongoDbHelper
	{
		private string _storeHostName;

		public IntegrationMongoDbHelper(string storeHostName)
		{
			_storeHostName = storeHostName;
		}

		public void StartMongo()
		{
			MongoRunner.Start();

			while (!Process.GetProcessesByName("mongod").Any())
			{
				Thread.Sleep(500);
			}
		}

		public void ShutdownMongo()
		{
			ShutdownMongo(RunnerConfiguration.Port);
		}

		public void ShutdownMongo(int port)
		{
			MongoRunner.Shutdown(port);
			WaitForShutdownToComplete();
		}

		public void ShutdownMongo(IList<int> ports)
		{
			if (!ports.Any())
				return;

			foreach (var port in ports)
			{
				MongoRunner.Shutdown(port);
			}

			WaitForShutdownToComplete();
			DeleteMongoDbTempDataFolder(ports);

			MongoServer.GetAllServers().ToList().ForEach(x => x.Disconnect());
		}

		public void DropDatabase()
		{
			var mongoUrl = new MongoUrl(_storeHostName);
			MongoServer.Create(mongoUrl).GetDatabase(mongoUrl.DatabaseName).Drop();
		}

		private void DeleteMongoDbTempDataFolder(IEnumerable<int> ports)
		{
			ports.ToList().ForEach(x => Directory.Delete("mongodb_" + x, true));
		}

		private static void WaitForShutdownToComplete()
		{
			while (Process.GetProcessesByName("mongod").Any())
			{
				Thread.Sleep(500);
			}

			while (Process.GetProcessesByName("mongod-killer").Any())
			{
				Thread.Sleep(500);
			}
		}
	}
}
