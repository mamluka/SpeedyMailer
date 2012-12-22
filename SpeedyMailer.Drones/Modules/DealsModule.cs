using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using SpeedyMailer.Core.Domain;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class DealsModule : NancyModule
	{
		public DealsModule(OmniRecordManager omniRecordManager)
			: base("/deals")
		{
			Get["/{data}"] = call =>
								 {
									 string objectString = call.data;
									 var data = UrlBuilder.DecodeBase64(objectString);

									 var creativeId = data.BuildRavenId("creatives", 0);
									 var contactId = data.BuildRavenId("contacts", 1);

									 var creativeToDealMap = omniRecordManager.Load<CreativeToDealMap>(creativeId);

									 omniRecordManager.UpdateOrInsert(new ClickAction
																		   {
																			   ContactId = contactId,
																			   CreativeId = creativeId,
																			   Date = DateTime.UtcNow
																		   });

									 return new RedirectResponse(creativeToDealMap.DealUrl, RedirectResponse.RedirectType.Permanent);
								 };
		}
	}
}
