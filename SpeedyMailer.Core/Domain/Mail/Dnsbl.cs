using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class Dnsbl
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public DnsnlType Type { get; set; }

		public string Dns { get; set; }
		public string Name { get; set; }
	}
}