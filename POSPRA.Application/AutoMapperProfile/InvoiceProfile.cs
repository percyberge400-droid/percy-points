using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.Application.AutoMapperProfile
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            CreateMap<InvoiceDto, Invoice>();
            CreateMap<InvoiceItemDetailDto, InvoiceItemDetail>();
        }
    }
}
