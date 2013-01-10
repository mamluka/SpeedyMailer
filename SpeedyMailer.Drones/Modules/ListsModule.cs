using System;
using Nancy;
using Nancy.Responses;
using SpeedyMailer.Core.Domain;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class ListsModule : NancyModule
	{
		public ListsModule(OmniRecordManager omniRecordManager)
			: base("/unsubscribe")
		{
			Get["/{data}"] = call =>
									  {
										  string objectString = call.data;
										  var data = UrlBuilder.DecodeBase64(objectString);

										  var creativeId = data.BuildRavenId("creatives", 0);
										  var contactId = data.BuildRavenId("contacts", 1);

										  omniRecordManager.UpdateOrInsert(new UnsubscribeRequest
																				{
																					ContactId = contactId,
																					CreativeId = creativeId,
																					Date = DateTime.UtcNow
																				});

										  return Response.AsText("You are now unsubscribed, have a nice day");
									  };

		}
	}
}
