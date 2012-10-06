using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Templates
		{
			public class CreateUnsubscribeTemplate : ApiCall
			{
				public string Body { get; set; }

				public CreateUnsubscribeTemplate()
					: base("/templates/unsubscribe")
				{
					CallMethod = RestMethod.Post;
				}
			}

			public class GetTemplates : ApiCall
			{
				public TemplateType Type { get; set; }

				public GetTemplates()
					: base("/templates/{type}")
				{
					CallMethod = RestMethod.Get;
				}
			}
		}
	}
}
