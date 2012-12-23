using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
	public class Creawtive_UnsubscribeRequestsIndex : IntegrationTestBase
	{
		[Test]
		public void Index_WhenGivenSnapShots_ShouldMapReduceUnsubscribeRequests()
		{
			var snapshots = new[]
				                {
					                new DroneStateSnapshoot
						                {
							                UnsubscribeRequests = new List<UnsubscribeRequest>
								                                      {
									                                      new UnsubscribeRequest {ContactId = "contact/1", CreativeId = "creative/1"},
									                                      new UnsubscribeRequest {ContactId = "contact/2", CreativeId = "creative/1"}
								                                      }
						                },
					                new DroneStateSnapshoot
						                {
							                UnsubscribeRequests = new List<UnsubscribeRequest>
								                                      {
									                                      new UnsubscribeRequest {ContactId = "contact/100", CreativeId = "creative/1"},
									                                      new UnsubscribeRequest {ContactId = "contact/200", CreativeId = "creative/1"}
								                                      }
						                },
					                new DroneStateSnapshoot
						                {
							                UnsubscribeRequests = new List<UnsubscribeRequest>
								                                      {
									                                      new UnsubscribeRequest {ContactId = "contact/1", CreativeId = "creative/2"},
									                                      new UnsubscribeRequest {ContactId = "contact/2", CreativeId = "creative/2"}
								                                      }
						                },
					                new DroneStateSnapshoot
						                {
							                UnsubscribeRequests = new List<UnsubscribeRequest>
								                                      {
									                                      new UnsubscribeRequest {ContactId = "contact/1000", CreativeId = "creative/1"},
									                                      new UnsubscribeRequest {ContactId = "contact/2", CreativeId = "creative/2"}
								                                      }
						                },
				                }.ToList();
			
			snapshots.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Creative_UnsubscribeRequests.ReduceResult, Creative_UnsubscribeRequests>();

			var result = Store.Query<Creative_UnsubscribeRequests.ReduceResult, Creative_UnsubscribeRequests>(x => x.CreativeId == "creative/1");

			result.Should().Contain(x => x.CreativeId == "creative/1");
			result.Should().OnlyContain(x => x.UnsubscribeRequests.Count() == 5);
		}
	}
}
