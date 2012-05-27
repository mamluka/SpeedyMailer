using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ninject;
using Ploeh.AutoFixture;
using Raven.Client;
using Raven.Client.Document;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core.Commands;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class Actions : ActionsBase
	{
		public Actions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor)
			: base(kernel, taskManager, taskExecutor)
		{ }

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

		public string CreateAListWithRandomContacts(string listName, int contactsCount)
		{
			var listId = ExecuteCommand<CreateListCommand, string>(x => x.Name = "MyList");
			ExecuteCommand<AddContactsCommand, long>(command =>
			{
				command.Contacts = Fixture
					.Build<Contact>()
					.Without(x => x.Id)
					.CreateMany(1500);

				command.ListId = listId;

			});
			return listId;
		}

		public string CreateSimpleCreative(IEnumerable<string> lists, string unsubscribeTemplateId)
		{
			return ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.Body = "body";
				x.Lists = lists.ToList();
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = unsubscribeTemplateId;
			});
		}
	}

	public class MasterActions : ActionsBase
	{
		private Service _service;

		public MasterActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor)
			: base(kernel, taskManager, taskExecutor)
		{ }

		public void Initialize()
		{
			var documentStore = Kernel.Get<IDocumentStore>();
			_service = new Service(new IntegrationNancyNinjectBootstrapper(documentStore));
		}
		public void Start()
		{

			_service.Start();
		}

		public void Stop()
		{
			_service.Stop();
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
