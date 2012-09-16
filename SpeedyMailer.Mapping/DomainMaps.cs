using AutoMapper;
using Bootstrap.AutoMapper;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Mapping
{
    public class DomainMaps : IMapCreator
    {

        public void CreateMap(IProfileExpression mapper)
        {
            Mapper.CreateMap<ContactsListCsvRow, Contact>()
                .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email));
        }

    }
}