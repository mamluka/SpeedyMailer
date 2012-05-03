using System;
using System.Linq.Expressions;
using Ninject;
using Raven.Client;
using Raven.Client.Document;
using SpeedyMailer.Master.Service;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    public class Actions : ActionsBase
    {
        public Actions(IKernel kernel):base(kernel)
        {}

    	public override void EditSettings<T>(Action<T> expression)
    	{
    		
    	}
    }

	public class ServiceActions : ActionsBase
	{
		private Service _service;

		public ServiceActions(IKernel kernel)
			: base(kernel)
		{}

		public void Start()
		{
			var documentStore = Kernel.Get<IDocumentStore>();
			_service = new Service(new IntegrationNancyNinjectBootstrapper(documentStore));
			_service.Stop();
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