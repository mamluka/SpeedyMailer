using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpeedyMailer.Core.Domain.Creative
{
    public class Template
    {
        public string Id { get; set; }
        public string Body { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
	    public TemplateType Type { get; set; }
    }
}