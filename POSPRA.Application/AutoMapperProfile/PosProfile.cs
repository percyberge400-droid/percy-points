using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.FiscalDtos;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;
using POSPRA.DTOs.ProductCatalogDtos;

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

            CreateMap<ProductCatalogueDto, ProductCatalogue>();
            CreateMap<ProductCatalogue, ProductCatalogueDto>();

            CreateMap<Logs, SyncLogDto>();
            CreateMap<SyncLogDto, Logs>();

            // DTO ➜ Entity
            // Parent mapping
            CreateMap<InvoiceDto, Invoice>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                // map child collection explicitly
                .ForMember(dest => dest.InvoiceItems,
                           opt => opt.MapFrom(src => src.InvoiceItemDto));

            // Child mapping
            CreateMap<InvoiceItemDto, InvoiceItems>()
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            //CreateMap<SyncLogDto, Logs>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0));
        }
    }
}
