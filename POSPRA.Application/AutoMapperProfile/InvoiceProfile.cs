using AutoMapper;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.DTOs.PosDTOs;

namespace POSPRA.Application.AutoMapperProfile
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            CreateMap<InvoiceDto, Invoice>();
            CreateMap<InvoiceItemDetailDto, InvoiceItemDetail>();

            CreateMap<Dictionary<string, object>, ResponseConfigurationDto>()
              .ConvertUsing<DictionaryToDtoConverter<ResponseConfigurationDto>>();

            // ✅ Add this for POSVerificationDTO
            CreateMap<Dictionary<string, object>, POSVerificationDTO>()
                .ConvertUsing<DictionaryToDtoConverter<POSVerificationDTO>>();
        }
    }
}
