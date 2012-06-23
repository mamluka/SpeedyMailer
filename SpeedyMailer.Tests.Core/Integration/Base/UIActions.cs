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
	public class UIActions : MasterActionsBase
	{
		public UIActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor)
			: base(kernel, taskManager, taskExecutor)
		{ }

		public string CreateListWithRandomContacts(string listName, int contactsCount)
		{
			var listId = ExecuteCommand<CreateListCommand, string>(x => x.Name = "MyList");
			ExecuteCommand<AddContactsCommand, long>(command =>
			{
				command.Contacts = Fixture
					.Build<Contact>()
					.Without(x => x.Id)
					.CreateMany(contactsCount);

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

	public class ServiceActions : MasterActionsBase
	{
		private TopService _topService;

		public ServiceActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor)
			: base(kernel, taskManager, taskExecutor)
		{ }

		public void Initialize()
		{
			var documentStore = Kernel.Get<IDocumentStore>();
			_topService = new TopService(new IntegrationNancyNinjectBootstrapper(documentStore));
		}
		public void Start()
		{

			_topService.Start();
		}

		public void Stop()
		{
			_topService.Stop();
		}
	}
}
