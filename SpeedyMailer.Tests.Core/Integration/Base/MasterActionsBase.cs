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
				var settings = Kernel.Get<T>();
				action.Invoke(settings);
				session.Store(settings);

				session.SaveChanges();
			}
		}
	}
}