using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Domain.Contacts;
using SpeedyMailer.Domain.Emails;
using SpeedyMailer.Domain.Lists;
using SpeedyMailer.Mapping.Resolve;
using SpeedyMailer.Master.Web.Core.ComponentViewModel;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Mapping
{
    public class ViewModelMaps : IMapCreator
    {
        #region IMapCreator Members

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

        #endregion
    }
}