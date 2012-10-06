using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Creative
		{
			public class FetchFragment : ApiCall
			{
				public FetchFragment()
					: base("/creative/fragments")
				{
					CallMethod = RestMethod.Get;
				}

				public class Response
				{
					public CreativeFragment CreativeFragment { get; set; }
				}
			}

			public class SaveCreative : ApiCall
			{
				public string Body { get; set; }
				public string ListId { get; set; }
				public string Subject { get; set; }
				public string DealUrl { get; set; }
				public string UnsubscribeTemplateId { get; set; }

				public SaveCreative()
					: base("/creative/save")
				{
					CallMethod = RestMethod.Post;
				}
			}

			public class Send : ApiCall
			{
				public Send()
					: base("/creative/send")
				{
					CallMethod = RestMethod.Post;
				}

				public string CreativeId { get; set; }

				public class Response
				{

				}
			}
		}
	}
}
