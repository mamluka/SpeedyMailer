using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Master.Service.Modules
{
	public class TemplatesModule : NancyModule
	{
		private readonly IDocumentStore _documentStore;

		public TemplatesModule(IDocumentStore documentStore, CreateTemplateCommand createTemplateCommand)
			: base("templates/{type}")
		{
			_documentStore = documentStore;
			Post["/"] = call =>
							{
								var model = this.Bind<ServiceEndpoints.CreateUnsubscribeTemplate>();

								createTemplateCommand.Body = model.Body;
								createTemplateCommand.Type = Enum.Parse(typeof(TemplateType), call.type, true);
								createTemplateCommand.Execute();

								return Response.AsText("OK");
							};

			Get["/"] = call =>
						   {
							   using (var session = _documentStore.OpenSession())
							   {
								   var type = (TemplateType)Enum.Parse(typeof(TemplateType), call.type, true);
								   return Response.AsJson(session.Query<Template>().Where(q => q.Type == type).ToList());
							   }
						   };
		}
	}
}
