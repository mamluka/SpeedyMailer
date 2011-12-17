using AutoMapper;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Emails;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Core.Tests.Maps
{
    public class AutoMapperMaps:IAutoMapperMaps
    {
        public void CreateMaps()
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
            ;
            Mapper.CreateMap<Email, EmailFragment>()
                .ForMember(x=> x.Recipients,opt=>opt.Ignore())
                .ForMember(x=> x.Locked,opt=>opt.UseValue(false))
                ;
        }
    }
}
  
