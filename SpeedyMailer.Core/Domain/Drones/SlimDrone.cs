using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class SlimDrone
	{
		public string Id { get; set; }
		public string Domain { get; set; }

		[JsonConverter(typeof(IsoDateTimeConverter))]
		public DateTime LastUpdated { get; set; }
	}
}