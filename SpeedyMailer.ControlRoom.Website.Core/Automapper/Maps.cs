using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Core.Automapper
{
    public class Maps : IMapCreator
    {
        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<EmailCSVParserResults, UploadListViewModel>()
                ;

            Mapper.CreateMap<UploadListModel, InitialEmailBatchOptions>()
                .ForMember(x=> x.ContainingListId,opt=> opt.MapFrom(x=> x.List))
                ;


        }
    }
}
