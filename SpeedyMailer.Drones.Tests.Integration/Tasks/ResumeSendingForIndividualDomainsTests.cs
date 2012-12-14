using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
    public class ResumeSendingForIndividualDomainsTests : IntegrationTestBase
    {
        public ResumeSendingForIndividualDomainsTests()
            : base(x => x.UseMongo = true)
        { }

        [Test]
        public void Execute_WhenThereAreProcessedMailThatWasPausedAndItsTimeToResumeIt_ShouldMakeAsUnprocessed()
        {
            DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

            var creativePackage = new List<CreativePackage>
                {
                    new CreativePackage
                        {
                            To = "david@david.com",
                            Group = "$default$",
                            Processed = true
                        },
                    new CreativePackage
                        {
                            To = "cookie@cookie.com",
                            Group = "$default$",
                            Processed = true
                        },
                };

            DroneActions.StoreCollection(creativePackage);

            DroneActions.Store(new GroupsAndIndividualDomainsSendingPolicies
                {
                    GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>
                        {
                            {"david.com", new ResumeSendingPolicy {ResumeAt = DateTime.UtcNow + TimeSpan.FromHours(-4)}},
                            {"cookie.com", new ResumeSendingPolicy {ResumeAt = DateTime.UtcNow + TimeSpan.FromHours(-2)}}
                        }
                });

            var task = new ResumeSendingForIndividualDomains();

            DroneActions.StartScheduledTask(task);
            Jobs.Drone().WaitForJobToStart(task);

            DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.To == "david@david.com", x => x.Processed == false);
            DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.To == "cookie@cookie.com", x => x.Processed == false);
        } 
		
		[Test]
        public void Execute_WhenThereAreProcessedMailAndThereAreNoMatchesOfDomainsToBeResumed_ShouldDoNothing()
        {
            DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

            var creativePackage = new List<CreativePackage>
                {
                    new CreativePackage
                        {
                            To = "david@david.com",
                            Group = "$default$",
                            Processed = true
                        },
                    new CreativePackage
                        {
                            To = "cookie@cookie.com",
                            Group = "$default$",
                            Processed = true
                        },
                };

            DroneActions.StoreCollection(creativePackage);

            DroneActions.Store(new GroupsAndIndividualDomainsSendingPolicies
                {
                    GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>
                        {
                            {"david.com", new ResumeSendingPolicy {ResumeAt = DateTime.UtcNow + TimeSpan.FromHours(4)}},
                            {"cookie.com", new ResumeSendingPolicy {ResumeAt = DateTime.UtcNow + TimeSpan.FromHours(2)}}
                        }
                });

            var task = new ResumeSendingForIndividualDomains();

            DroneActions.StartScheduledTask(task);
            Jobs.Drone().WaitForJobToStart(task);

			Thread.Sleep(1000);

            DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.To == "david@david.com", x => x.Processed);
            DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.To == "cookie@cookie.com", x => x.Processed);
        }

        [Test]
        public void Execute_WhenNotAllPackagesNeedsToBeResume_ShouldOnlyResumeTheOnesThatAreAtTheRightTime()
        {
            DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

            var creativePackage = new List<CreativePackage>
                {
                    new CreativePackage
                        {
                            To = "david@david.com",
                            Group = "$default$",
                            Processed = true
                        },
                    new CreativePackage
                        {
                            To = "cookie@cookie.com",
                            Group = "$default$",
                            Processed = true
                        },
                };

            DroneActions.StoreCollection(creativePackage);

            DroneActions.Store(new GroupsAndIndividualDomainsSendingPolicies
                {
                    GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>
                        {
                            {"david.com", new ResumeSendingPolicy {ResumeAt = DateTime.UtcNow + TimeSpan.FromHours(-4)}},
                            {"cookie.com", new ResumeSendingPolicy {ResumeAt = DateTime.UtcNow + TimeSpan.FromHours(2)}}
                        }
                });

            var task = new ResumeSendingForIndividualDomains();

            DroneActions.StartScheduledTask(task);
            Jobs.Drone().WaitForJobToStart(task);

            DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.To == "david@david.com", x => x.Processed == false);
            DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.To == "cookie@cookie.com", x => x.Processed);
        }
    }
}
