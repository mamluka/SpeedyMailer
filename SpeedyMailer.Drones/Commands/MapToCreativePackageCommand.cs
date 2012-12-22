using System;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Master;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using Template = Antlr4.StringTemplate.Template;

namespace SpeedyMailer.Drones.Commands
{
	public class MapToCreativePackageCommand : Command<CreativePackage>
	{
		private readonly CreativeBodySourceWeaver _creativeBodySourceWeaver;
		private readonly UrlBuilder _urlBuilder;
		private readonly DroneSettings _droneSettings;

		public Recipient Recipient { get; set; }
		public CreativeFragment CreativeFragment { get; set; }

		public MapToCreativePackageCommand(UrlBuilder urlBuilder, CreativeBodySourceWeaver creativeBodySourceWeaver, DroneSettings droneSettings)
		{
			_droneSettings = droneSettings;
			_urlBuilder = urlBuilder;
			_creativeBodySourceWeaver = creativeBodySourceWeaver;
		}

		public override CreativePackage Execute()
		{
			return ToPackage(Recipient, CreativeFragment);
		}

		private CreativePackage ToPackage(Recipient recipient, CreativeFragment creativeFragment)
		{
			return new CreativePackage
					{
						Subject = creativeFragment.Subject,
						HtmlBody = PersonalizeBody(creativeFragment, recipient),
						TextBody = PersonalizeBody(creativeFragment, recipient),
						To = recipient.Email,
						Group = recipient.Group,
						FromName = creativeFragment.FromName,
						FromAddressDomainPrefix = creativeFragment.FromAddressDomainPrefix,
						Interval = recipient.Interval,
						CreativeId = creativeFragment.CreativeId
					};
		}

		private string PersonalizeBody(CreativeFragment fragment, Recipient contact)
		{
			var dealUrl = _urlBuilder
				.Base(_droneSettings.BaseUrl + "/deals")
				.AddString(GetDealUrlData(fragment, contact))
				.AppendAsSlashes();

			var unsubsribeUrl = _urlBuilder
				.Base(_droneSettings.BaseUrl + "/unsubscribe")
				.AddString(GetDealUrlData(fragment, contact))
				.AppendAsSlashes();

			var bodyTemplateEngine = new Template(fragment.HtmlBody, '^', '^');
			bodyTemplateEngine.Add("email", contact.Email);

			var body = bodyTemplateEngine.Render();

			var weavedBody = _creativeBodySourceWeaver.WeaveDeals(body, dealUrl);

			var unsubscribeTemplateEngine = new Template(fragment.UnsubscribeTemplate, '^', '^');
			unsubscribeTemplateEngine.Add("url", unsubsribeUrl);

			var template = unsubscribeTemplateEngine.Render();

			return weavedBody + template;
		}

		private static string GetDealUrlData(CreativeFragment fragment, Recipient contact)
		{
			return fragment.CreativeId.StripRavenId() + "," + contact.ContactId.StripRavenId();
		}
	}

	internal class PackageInfo
	{
		public string Group { get; set; }

		public int Interval { get; set; }

		public int Count { get; set; }
	}
}