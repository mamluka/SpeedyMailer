using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Domain.Email;
using Raven.Client.Linq;

namespace SpeedyMailer.Master.Service.Modules
{
	public class DealsModule : NancyModule
	{
		public DealsModule(IDocumentStore documentStore)
			: base("/deals")
		{
			Get["/"] = call =>
						   {
							   using (var session = documentStore.OpenSession())
							   {
								   var creative = session.Query<Creative>().FirstOrDefault();

								   if (creative == null)
									   return new NotFoundResponse();

								   return new RedirectResponse(creative.DealUrl, RedirectResponse.RedirectType.Permanent);
							   }
						   };

			Get["/{data}"] = call =>
								 {
									 string objectString = call.data;

									 var data = UrlBuilder.DecodeBase64<DealUrlData>(objectString);

									 using (var session = documentStore.OpenSession())
									 {
										 var creative = session.Load<Creative>(data.CreativeId);

										 if (creative == null)
											 return new NotFoundResponse();

										 var contactActions = session
											 .Query<ContactActions>()
											 .Where(q => q.ContactId == data.ContactId)
											 .FirstOrDefault();

										 if (contactActions == null)
										 {
											 contactActions = new ContactActions
																  {
																	  Clicks = new List<string> { data.CreativeId },
																	  ContactId = data.ContactId
																  };
										 }
										 else
										 {
											 contactActions.Clicks.Add(creative.Id);
										 }

										 session.Store(contactActions);
										 session.SaveChanges();

										 return new RedirectResponse(creative.DealUrl, RedirectResponse.RedirectType.Permanent);
									 }
								 };
		}
	}
}
