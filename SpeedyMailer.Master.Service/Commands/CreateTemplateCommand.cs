using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Service.Commands
{
	public class CreateTemplateCommand : Command<string>
	{
		private readonly IDocumentStore _documentStore;
		public string Body { get; set; }
		public TemplateType Type { get; set; }

		public CreateTemplateCommand(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override string Execute()
		{
			using (var session = _documentStore.OpenSession())
			{
				var template = new Template
								   {
									   Body = Body,
									   Type = Type
								   };

				session.Store(template);
				session.SaveChanges();
				return template.Id;
			}
		}

	}
}