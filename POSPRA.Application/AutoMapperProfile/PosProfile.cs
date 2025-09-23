using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.LogDtos;

namespace POSPRA.Application.AutoMapperProfile
{
    public class PosProfile : Profile
    {
        public PosProfile()
        {
            CreateMap<FileRecordDto, FileRecord>();
            CreateMap<FileRecord, FileRecordDto>();

            CreateMap<LogDto, Logs>();
            CreateMap<Logs, LogDto>();

            CreateMap<WorkerLogDto, Logs>();
            CreateMap<Logs, WorkerLogDto>();

            // DTO ➜ Entity
            CreateMap<InvoiceDto, Invoice>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<InvoiceItemDto, InvoiceItems>()
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));
        }
    }
}
