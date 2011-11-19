using AutoMapper;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Core.Tests.Maps
{
    public static class AutoMapperMaps
    {
        public static void CreateMaps()
        {
            Mapper.CreateMap<EmailFromCSVRow, Email>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
                ;
        }
    }
}
