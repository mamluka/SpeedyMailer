using AutoMapper;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.EmailPoolMaster.Tests.Maps
{
    public class AutoMapperMaps:IAutoMapperMaps
    {
        public void CreateMaps()
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
            ;
            Mapper.CreateMap<Email, EmailFragment>()
                .ForMember(x=> x.ExtendedRecipients,opt=>opt.Ignore())
                .ForMember(x=> x.Locked,opt=>opt.UseValue(false))
                ;
        }
    }
}
  
