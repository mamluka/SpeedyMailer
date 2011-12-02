using AutoMapper;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Core.Tests.Maps
{
    public class AutoMapperMaps:IAutoMapperMaps
    {
        public void CreateMaps()
        {
            Mapper.CreateMap<EmailFromCSVRow, Email>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
            ;
        }
    }
}
  
