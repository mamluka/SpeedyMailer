using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Runner;
using MongoDB.Runner.Configuration;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationMongoDbHelper
	{
		public void StartMongo()
		{
			MongoRunner.Start();

			try
			{
				var manager = new GenericRecordManager<SocketCycling>(IntergrationHelpers.DefaultStoreUri());
				manager.BatchInsert(new[] { new SocketCycling(), });
			}
			catch (Exception)
			{
				Trace.WriteLine("Mongo socket was garbage collected");
			}

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

		}

		private void DeleteMongoDbTempDataFolder(IEnumerable<int> ports)
		{
			ports
				.ToList()
				.ForEach(x => Directory.Delete("mongodb_" + x, true));
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
