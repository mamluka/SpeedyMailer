using System.Collections.Generic;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Rules;

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

				public string DroneId { get; set; }

				public class Response
				{
					public CreativeFragment CreativeFragment { get; set; }
				}
			}

			public class SaveCreative : ApiCall
			{
				public string HtmlBody { get; set; }
				public string ListId { get; set; }
				public string Subject { get; set; }
				public string DealUrl { get; set; }
				public string UnsubscribeTemplateId { get; set; }
				public string FromName { get; set; }
				public string FromAddressDomainPrefix { get; set; }
				public string TextBody { get; set; }

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
			}

			public class GetAll:ApiCall
			{
				public GetAll():base("/creative/getall")
				{
					CallMethod = RestMethod.Get;
				}
			}
		}
	}
}
