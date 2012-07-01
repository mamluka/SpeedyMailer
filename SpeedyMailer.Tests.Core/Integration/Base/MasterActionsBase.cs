using System;
using System.Linq.Expressions;
using Ninject;
using Ploeh.AutoFixture;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public abstract class MasterActionsBase : ActionsBase
	{
		public readonly IKernel Kernel;
		private readonly ITaskManager _taskManager;
		private readonly ITaskExecutor _taskExecutor;
		private readonly IScheduledTaskManager _scheduledTaskManager;

		public Fixture Fixture { get; private set; }

		protected MasterActionsBase(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor,IScheduledTaskManager scheduledTaskManager):base(kernel)
		{
			_scheduledTaskManager = scheduledTaskManager;
			_taskExecutor = taskExecutor;
			_taskManager = taskManager;
			Kernel = kernel;
			Fixture = new Fixture();
		}

		public void SaveTask(PersistentTask task)
		{
			_taskManager.Save(task);
		}

		public string ExecuteTask(PersistentTask task)
		{
			var taskId = _taskManager.Save(task);
			_taskExecutor.Start();
			return taskId;
		}

		public void StartScheduledTask(ScheduledTask scheduledTask)
		{
			_scheduledTaskManager.AddAndStart(scheduledTask);
		}

		public override void EditSettings<T>(Action<T> action)
		{
			var documentStore = Kernel.Get<IDocumentStore>();
			using (var session = documentStore.OpenSession())
			{
				var settings = Kernel.Get<T>();
				action.Invoke(settings);
				session.Store(settings);

				session.SaveChanges();
			}
		}
	}
}