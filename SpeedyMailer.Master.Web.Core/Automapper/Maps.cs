using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Domain.Model.Contacts;
using SpeedyMailer.Domain.Model.Emails;
using SpeedyMailer.Domain.Model.Lists;
using SpeedyMailer.Master.Web.Core.ComponentViewModel;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.Core.Automapper
{
    public class Maps : IMapCreator
    {
        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
            ;

            Mapper.CreateMap<ContactCSVParserResults, UploadListViewModel>()
                .ForMember(dto => dto.Results, opt => opt.ResolveUsing<FromUploadResultsToViewModel>())
                ;

            Mapper.CreateMap<UploadListModel, InitialContactsBatchOptions>()
                .ForMember(x => x.ContainingListId, opt => opt.MapFrom(x => x.List))
                ;

            Mapper.CreateMap<ListDescriptor, ListDescriptorViewModel>()
                ;

            Mapper.CreateMap<ComposeModel, Email>()
                ;
            Mapper.CreateMap<Email, EmailFragment>()
               .ForMember(x => x.Id, opt => opt.Ignore())
               ;
        }
    }
}
