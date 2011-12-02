using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.ControlRoom.Website.Tests.Maps
{
    public class AutoMapperMaps:IAutoMapperMaps
    {
        public void CreateMaps()
        {
            Mapper.CreateMap<EmailFromCSVRow, Email>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
                ;

                Mapper.CreateMap<EmailCSVParserResults, UploadListViewModel>();
        }
    }
}
