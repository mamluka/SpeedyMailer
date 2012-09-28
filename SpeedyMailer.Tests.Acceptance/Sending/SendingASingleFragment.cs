using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Tests.Acceptance.Sending
{
	[TestFixture]
	public class SendingASingleFragment:IntegrationTestBase
	{
		[Test]
		public void WhenSendingTheCreativeShouldSendTheFragments()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.Initialize("http://localhost:5487",DefaultBaseUrl);
			DroneActions.Start();
		}
	}
}
