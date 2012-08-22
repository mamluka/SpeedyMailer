using System;
using Ninject;
using Ploeh.AutoFixture;
using Raven.Client;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service.Container;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public abstract class MasterActionsBase : ActionsBase
	{
		public readonly IKernel Kernel;

		public Fixture Fixture { get; private set; }

		protected MasterActionsBase(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor, IScheduledTaskManager scheduledTaskManager)
			: base(kernel,taskManager,taskExecutor,scheduledTaskManager)
		{
			Kernel = kernel;
			Fixture = new Fixture();
		}

		public override void EditSettings<T>(Action<T> action)
		{
			var documentStore = Kernel.Get<IDocumentStore>();
			using (var session = documentStore.OpenSession())
			{
				var settings = new T();
				action.Invoke(settings);
				session.Store(settings,"settings/" + typeof(T).Name.Replace("Settings",""));
				session.SaveChanges();
			}

			ContainerBootstrapper.ReloadStoreSetting<T>(Kernel,documentStore);
		}
	}
}