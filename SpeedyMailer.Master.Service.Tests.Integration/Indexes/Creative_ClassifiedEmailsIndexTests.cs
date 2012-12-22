using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
	public class Creative_ClassifiedEmailsIndexTests : IntegrationTestBase
	{
		[Test]
		public void Index_WhenGivenSnapShots_ShouldMapReduceAllUnclassified()
		{
			var snapshots = new[]
				{
					new DroneStateSnapshoot
						{
							Unclassified = new List<UnclassfiedMailEvent>
								{
									new UnclassfiedMailEvent
										{
											Recipient = "david@david.com",
											CreativeId = "creatives/1"
										}
								}
						},
					new DroneStateSnapshoot
						{
							Unclassified = new List<UnclassfiedMailEvent>
								{
									new UnclassfiedMailEvent
										{
											Recipient = "david@sales.com",
											CreativeId = "creatives/1"
										}
								}
						}
				}.ToList();

			snapshots.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Creative_UnclassifiedEmails.ReduceResult, Creative_UnclassifiedEmails>();

			var result = Store.Query<Creative_UnclassifiedEmails.ReduceResult, Creative_UnclassifiedEmails>(x => x.CreativeId == "creatives/1");

			result.Should().Contain(x => x.CreativeId == "creatives/1");
			result[0].Unclassified.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@david.com", "david@sales.com" });
		}
	}
}
