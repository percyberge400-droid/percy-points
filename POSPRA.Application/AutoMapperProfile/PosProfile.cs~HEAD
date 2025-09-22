using AutoMapper;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.DTOs.LogDTOs;
using POSPRA.DTOs.PosDTOs;

namespace POSPRA.Application.AutoMapperProfile
{
    public class PosProfile : Profile
    {
        public PosProfile()
        {
            CreateMap<InvoiceDto, Invoice>();
            CreateMap<InvoiceItemDetailDto, InvoiceItemDetail>();

            CreateMap<FileRecordDTO, FileRecord>();
            CreateMap<FileRecord, FileRecordDTO>();

            CreateMap<LogDto, Logs>();
            CreateMap<Logs, LogDto>();

            CreateMap<WorkerLogDTO, Logs>();
            CreateMap<Logs, WorkerLogDTO>();

            CreateMap<PosConfigurationDto, PosConfiguration>();
            CreateMap<PosConfiguration, PosConfigurationDto>();

            // ✅ Add this for POSVerificationDTO
            CreateMap<Dictionary<string, object>, POSVerificationDTO>()
                .ConvertUsing<DictionaryToDtoConverter<POSVerificationDTO>>();
        }
    }
}
