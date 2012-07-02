using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Ploeh.AutoFixture;
using Raven.Client;
using Raven.Client.Document;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core.Commands;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class UIActions : MasterActionsBase
	{
		public UIActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor,IScheduledTaskManager scheduledTaskManager)
			: base(kernel, taskManager, taskExecutor,scheduledTaskManager)
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

		public string Hostname { get; set; }

		public ServiceActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor,IScheduledTaskManager scheduledTaskManager)
			: base(kernel, taskManager, taskExecutor,scheduledTaskManager)
		{ }

		public void Initialize(string hostname="")
		{
			if (String.IsNullOrEmpty(hostname))
				hostname = Hostname;

			EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = hostname);
			var documentStore = Kernel.Get<IDocumentStore>();
			Kernel.Rebind<INancyBootstrapper>().ToConstant(new ServiceNancyNinjectBootstrapperForTesting(documentStore) as INancyBootstrapper);
			_topService = Kernel.Get<TopService>();
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
