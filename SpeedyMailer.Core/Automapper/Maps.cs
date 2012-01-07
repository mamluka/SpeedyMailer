using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Core.Contacts;

namespace SpeedyMailer.Core.Automapper
{
    public class Maps : IMapCreator
    {
        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactFromCSVRow, Contact>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Email))
                ;




  
        }
    }
}
