using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.AutoMapperProfile
{
    public class PosProfile : Profile
    {
        public PosProfile()
        {
            CreateMap<InvoiceDto, Invoice>();
            CreateMap<InvoiceItemDetailDto, InvoiceItems>();

            CreateMap<FileRecordDTO, FileRecord>();
            CreateMap<FileRecord, FileRecordDTO>();

            CreateMap<LogDto, Logs>();
            CreateMap<Logs, LogDto>();

            CreateMap<WorkerLogDTO, Logs>();
            CreateMap<Logs, WorkerLogDTO>();

            //CreateMap<Dictionary<string, object>, ResponseConfigurationDto>()
            //  .ConvertUsing<DictionaryToDtoConverter<ResponseConfigurationDto>>();

            //// ✅ Add this for POSVerificationDTO
            //CreateMap<Dictionary<string, object>, POSVerificationDTO>()
            //    .ConvertUsing<DictionaryToDtoConverter<POSVerificationDTO>>();
        }
    }
}
