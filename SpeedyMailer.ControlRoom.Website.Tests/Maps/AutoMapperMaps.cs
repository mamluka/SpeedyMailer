using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.Automapper;
using SpeedyMailer.ControlRoom.Website.Core.ComponentViewModel;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Lists;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.ControlRoom.Website.Tests.Maps
{
    public class AutoMapperMaps:IAutoMapperMaps
    {
        public void CreateMaps()
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
            ;

            Mapper.CreateMap<ContactCSVParserResults, UploadListViewModel>()
                .ForMember(dto=> dto.Results,opt=> opt.ResolveUsing<FromUploadResultsToViewModel>())
                ;

            Mapper.CreateMap<UploadListModel, InitialContactsBatchOptions>()
                .ForMember(x => x.ContainingListId, opt => opt.MapFrom(x => x.List))
                ;

            Mapper.CreateMap<ListDescriptor, ListDescriptorViewModel>()
                ;

            Mapper.CreateMap<ComposeModel, Email>()
                ;

        }
    }
}
