using System;
using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Domain.Contacts;
using SpeedyMailer.Domain.Emails;

namespace SpeedyMailer.Mapping
{
    public class DomainMaps : IMapCreator
    {

        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
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