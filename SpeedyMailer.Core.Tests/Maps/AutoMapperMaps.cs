using AutoMapper;
using SpeedyMailer.Core.Contacts;
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
        }
    }
}
  
