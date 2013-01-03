using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class ReinstateRecipientsForSendingTests : IntegrationTestBase
	{
		public ReinstateRecipientsForSendingTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Happend_WhenBouncedMailIsClassifiedAsBlocked_ShouldKeepThePackageProcessed()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new CreativePackage
								   {
									   Processed = true,
									   To = "david@david.com"
								   });

			FireEvent<ReinstateRecipientsForSending,
				AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
					{
						new MailBounced
							{
								Message = "bounced that blocked",
								DomainGroup = "gmail",
								Recipient = "david@david.com",
								Classification = new MailClassfication {Classification = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
							}
					});

			DroneActions.WaitForDocumentToExist<CreativePackage>();

			var result = DroneActions.FindSingle<CreativePackage>();

			result.Processed.Should().BeTrue();
			result.To.Should().Be("david@david.com");
		}

		[Test]
		public void Happend_WhenBouncedMailIsWereNotClassified_ShouldSetThePastCreativePackageToBeNotProcessed()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new CreativePackage
								   {
									   Processed = true,
									   To = "david@david.com"
								   });

			FireEvent<ReinstateRecipientsForSending,
				AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
					                                           {
						                                           new MailBounced
							                                           {
								                                           Message = "bounced that blocked",
								                                           DomainGroup = "gmail",
								                                           Recipient = "david@david.com",
																		   Classification = new MailClassfication {Classification = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
							                                           }
					                                           });

			DroneActions.WaitForDocumentToExist<CreativePackage>();

			var result = DroneActions.FindSingle<CreativePackage>();

			result.Processed.Should().BeFalse();
			result.To.Should().Be("david@david.com");
		}

		[Test]
		public void Happend_WhenBouncedMailIsClassifiedAsBounce_ShouldSNotChangeTheProcess()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new CreativePackage
								   {
									   Processed = true,
									   To = "david@david.com"
								   });

			DroneActions.Store(new DeliverabilityClassificationRules
								   {
									   Rules = new List<HeuristicRule>
										   {
											   new HeuristicRule { Condition = "bounced",Type = Classification.HardBounce }
										   }
								   });

			FireEvent<ReinstateRecipientsForSending,
				AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
					                                           {
						                                           new MailBounced
							                                           {
								                                           Message = "bounced",
								                                           DomainGroup = "gmail",
								                                           Recipient = "david@david.com"
							                                           }
					                                           });

			DroneActions.WaitForDocumentToExist<CreativePackage>();

			var result = DroneActions.FindSingle<CreativePackage>();

			result.Processed.Should().BeTrue();
			result.To.Should().Be("david@david.com");
		}
	}
}
