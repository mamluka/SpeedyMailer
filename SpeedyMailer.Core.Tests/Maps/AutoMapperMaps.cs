using System;
using AutoMapper;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Emails;
using SpeedyMailer.Domain.Model.Contacts;
using SpeedyMailer.Domain.Model.Emails;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Core.Tests.Maps
{
    public class AutoMapperMaps:IAutoMapperMaps
    {
        public void CreateMaps()
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email));
            ;
            Mapper.CreateMap<Email, EmailFragment>()
                .ForMember(x=> x.ExtendedRecipients,opt=>opt.Ignore())
                .ForMember(x=> x.Locked,opt=>opt.UseValue(false))
                .ForMember(x => x.CreateDate, opt => opt.UseValue(DateTime.Now.Ticks))
                ;
        }
    }
}
  
