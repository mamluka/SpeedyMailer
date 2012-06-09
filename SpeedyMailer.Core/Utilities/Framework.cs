using System;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Core.Utilities
{
	public class Framework
	{
		private readonly ITaskManager _taskManager;
		private readonly IDocumentStore _documentStore;
		private ITaskCoordinator _taskCoordinator;

		public Framework(IDocumentStore documentStore, ITaskManager taskManager,ITaskCoordinator taskCoordinator)
		{
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
	}
}