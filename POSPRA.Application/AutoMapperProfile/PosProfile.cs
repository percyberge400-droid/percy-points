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
            CreateMap<FileRecordDTO, FileRecord>();
            CreateMap<FileRecord, FileRecordDTO>();

            CreateMap<LogDto, Logs>();
            CreateMap<Logs, LogDto>();

            CreateMap<WorkerLogDTO, Logs>();
            CreateMap<Logs, WorkerLogDTO>();

            // DTO ➜ Entity
            CreateMap<InvoiceDto, Invoice>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<InvoiceItemDto, InvoiceItems>()
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));
        }
    }
}
