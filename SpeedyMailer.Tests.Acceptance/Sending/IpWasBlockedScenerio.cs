using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Master.Service.Apis;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Tests.Acceptance.Sending
{
	public class IpWasBlockedScenerio : IntegrationTestBase
	{
		private Api _api;

		[Test]
		public void WhenSendingEmailAndOnOfTheEmailIsDeferredBecauseOfBadIp_ShouldStopTheSendingForThatParticularGroup()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.DefaultInterval = 2);

			ServiceActions.Initialize();
			ServiceActions.Start();

			_api = MasterResolve<Api>();

			AddIntervalRule("gmail", "gmail.com", 3);
			AddIntervalRule("hotmail", "hotmail.com", 3);

			var csvRows = Fixture
				.Build<ContactsListCsvRow>()
				.Without(x => x.Email)
				.CreateMany(30)
				.ToList();

			csvRows.Take(10).ToList().ForEach(x => x.Email = "email" + Guid.NewGuid() + "@domain.com");
			csvRows.Skip(10).Take(10).ToList().ForEach(x => x.Email = "email" + Guid.NewGuid() + "@gmail.com");
			csvRows.Skip(20).Take(10).ToList().ForEach(x => x.Email = "email" + Guid.NewGuid() + "@hotmail.com");

			CreateTemplate();
			CreateList("my list");

			AddContactsToList("my list", csvRows);
			var creativeId = SaveCreative();

			AddClassifictionRulesForBlockedIp("gmail has blocked you");
			SendCreative(creativeId);

			var drone = DroneActions.CreateDrone("drone1", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone.Initialize();
			drone.Start();

			Email.AssertEmailsSentTo(csvRows.Skip(10).Take(3).Select(x => x.Email).ToList(), 20);

			DroneActions.Store(new MailLogEntry
								   {
									   Level = "INFO",
									   Time = DateTime.UtcNow,
									   Msg = " B1F58AE39F: to=<lorihooks@gmail.com>, relay=none, delay=405978, delays=405873/0.02/105/0, dsn=4.4.1, status=deferred (gmail has blocked you)"
								   }, "drone1");

			_api.Call<DroneEndpoints.Admin.FireTask>(x => x.Job = typeof(AnalyzePostfixLogsTask).Name);

			Email.AssertEmailSent(23);
			Email.AssertEmailsSentTo(csvRows.Take(10).Select(x => x.Email).ToList());
			Email.AssertEmailsSentTo(csvRows.Skip(10).Take(3).Select(x => x.Email).ToList());
			Email.AssertEmailsSentTo(csvRows.Skip(20).Take(10).Select(x => x.Email).ToList());
		}

		private void AddClassifictionRulesForBlockedIp(string rule)
		{
			_api.Call<ServiceEndpoints.Heuristics.SaveDelivery>(x =>
																	{
																		x.IpBlockingRules = new List<string> { rule };

																	});
		}

		private void AddIntervalRule(string group, string match, int interval)
		{
			_api.Call<ServiceEndpoints.Rules.AddIntervalRules>(x => x.IntervalRules = new List<IntervalRule>
				                                                                          {
					                                                                          new IntervalRule
						                                                                          {
							                                                                          Group = group,
							                                                                          Interval = interval,
							                                                                          Conditons = new List<string>
								                                                                                      {
									                                                                                      match
								                                                                                      }
						                                                                          }
				                                                                          });
		}

		private void SendCreative(string creativeId)
		{
			_api.Call<ServiceEndpoints.Creative.Send>(x => x.CreativeId = creativeId);
		}

		private string SaveCreative()
		{
			var lists = _api.Call<ServiceEndpoints.Lists.GetLists, List<ListDescriptor>>();
			var templates = _api.Call<ServiceEndpoints.Templates.GetTemplates, List<Template>>(x => x.Type = TemplateType.Unsubscribe);

			var result = _api.Call<ServiceEndpoints.Creative.SaveCreative, ApiStringResult>(x =>
			{
				x.ListId = lists[0].Id;
				x.UnsubscribeTemplateId = templates[0].Id;
				x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
				x.Subject = "hello world subject";
				x.FromName = "david";
				x.FromAddressDomainPrefix = "sales";
			});

			return result.Result;
		}

		private string CreateBodyWithLink(string link)
		{
			return string.Format(@"<html><body>this email has a link inside of it <a href="" {0} "" >test link</as>""</body></html>", link);
		}

		private void AddContactsToList(string listName, IEnumerable<ContactsListCsvRow> csvRows)
		{
			var lists = _api.Call<ServiceEndpoints.Lists.GetLists, List<ListDescriptor>>();

			var fileName = CsvTestingExtentions.GenerateFileName("sample");

			csvRows.ToCsvFile(fileName);

			_api.AddFiles(new[] { fileName }).Call<ServiceEndpoints.Lists.UploadContacts>(x =>
			{
				x.ListId = lists[0].Id;
			});

		}

		private void CreateList(string listName)
		{
			_api.Call<ServiceEndpoints.Lists.CreateList>(x =>
			{
				x.Name = listName;
			});
		}

		private void CreateTemplate()
		{
			_api.Call<ServiceEndpoints.Templates.CreateUnsubscribeTemplate>(x =>
			{
				x.Body = "here is my template <url>";
			});
		}
	}
}
