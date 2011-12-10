using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ComponentViewModel;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Core.Automapper
{
    public class Maps : IMapCreator
    {
        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactCSVParserResults, UploadListViewModel>()
                ;

            Mapper.CreateMap<UploadListModel, InitialContactsBatchOptions>()
                .ForMember(x=> x.ContainingListId,opt=> opt.MapFrom(x=> x.List))
                ;

            Mapper.CreateMap<ListDescriptor, ListDescriptorViewModel>()

                ;
        }
    }
}
