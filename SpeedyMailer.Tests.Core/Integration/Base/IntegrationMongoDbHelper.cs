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
		private IKernel _droneKernel;

		public IntegrationMongoDbHelper(IKernel droneKernel)
		{
			_droneKernel = droneKernel;
		}

		public void StartMongo()
		{
			MongoRunner.Start();

			var waitForMongoToStart = true;

			while (waitForMongoToStart)
			{
				try
				{
//					var manager = new GenericRecordManager<SocketCycling>(IntergrationHelpers.DefaultStoreUri());
//					manager.BatchInsert(new[] { new SocketCycling() });
//					new[] {typeof (CoreAssemblyMarker).Assembly, typeof (DroneAssemblyMarker).Assembly}
//						.SelectMany(x => x.GetExportedTypes())
//						.Where(x => x.GetInterfaces().Any(i => i == typeof (ICycleSocket)))
//						.ToList()
//						.ForEach(x =>
//							         {
//								         var store = _droneKernel.Get(x) as ICycleSocket;
//								         store.CycleSocket();
//							         });

					var server = MongoServer.Create(new MongoUrl("mongodb://localhost:27027/drone?safe=true"));
					server.Connect();


					waitForMongoToStart = false;
				}
				catch (Exception ex)
				{
					Trace.WriteLine("Mongo socket was garbage collected " + ex.Message);

					Thread.Sleep(500);

					waitForMongoToStart = true;
				}
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
