using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Core.Utilities.Domain.Contacts;
using SpeedyMailer.Mapping.Resolve;
using SpeedyMailer.Master.Web.Core.ComponentViewModel;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Mapping
{
    public class ViewModelMaps : IMapCreator
    {

        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactsListCsvRow, Contact>()
                .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email));
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
        }

    }
}