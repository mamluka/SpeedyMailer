using System;
using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Utilities.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Commands;

namespace SpeedyMailer.Mapping
{
    public class DomainMaps : IMapCreator
    {

        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email));
            ;
            Mapper.CreateMap<Email, EmailFragment>()
                .ForMember(x => x.ExtendedRecipients, opt => opt.Ignore())
                .ForMember(x => x.Locked, opt => opt.UseValue(false))
                .ForMember(x => x.CreateDate, opt => opt.UseValue(DateTime.Now.Ticks))
                .ForMember(x => x.MailId, opt => opt.MapFrom(x => x.Id))
                ;
        }

    }
}