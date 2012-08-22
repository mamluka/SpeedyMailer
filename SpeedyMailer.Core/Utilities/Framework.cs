using System;
using System.IO;
using Newtonsoft.Json;
using Ninject;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Core.Utilities
{
	public class Framework
	{
		private readonly ITaskManager _taskManager;
		private readonly IDocumentStore _documentStore;
		private readonly ITaskCoordinator _taskCoordinator;
		private readonly IKernel _kernel;

		public Framework(IDocumentStore documentStore, ITaskManager taskManager, ITaskCoordinator taskCoordinator,IKernel kernel)
		{
			_kernel = kernel;
			_taskCoordinator = taskCoordinator;
			_documentStore = documentStore;
			_taskManager = taskManager;
		}

		public void ExecuteCommand(Command command)
		{
			command.Execute();
		}

		public TResult ExecuteCommand<TResult>(Command<TResult> command)
		{
			return command.Execute();
		}

		public void ExecuteTask(PersistentTask task)
		{
			_taskManager.Save(task);
			_taskCoordinator.BeginExecuting();
		}

		public void Store(dynamic entity)
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(entity);
				session.SaveChanges();
			}
		}

		public T Load<T>(string entityId)
		{
			using (var session = _documentStore.OpenSession())
			{
				return session.Load<T>(entityId);
			}
		}

		public void EditJsonSettings<T>(Action<T> action) where T : new()
		{
			const string settingsFolderName = "settings";

			if (!Directory.Exists(settingsFolderName))
				Directory.CreateDirectory(settingsFolderName);

			using (var writter = new StreamWriter(Path.Combine(settingsFolderName, SettingsFileName<T>())))
			{
				dynamic settings = new T();
				action(settings);
				writter.WriteLine(JsonConvert.SerializeObject(settings,
															  Formatting.Indented,
															  new JsonSerializerSettings
															  {
																  NullValueHandling = NullValueHandling.Ignore
															  }));
			}

			ContainerBootstrapper.ReloadJsonSetting<T>(_kernel);
		}

		private static string SettingsFileName<T>()
		{
			return typeof(T).Name.Replace("Settings", "") + ".settings";
		}
	}
}