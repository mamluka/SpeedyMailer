using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Core.Protocol
{
    public class CreativeApi
    {
        public class Add
        {
            public class Request
            {
                public string CreativeId { get; set; }

                public string UnsubscribedTemplateId { get; set; }
            }

            public class Response
            {
                 
            }
        }
    }
}