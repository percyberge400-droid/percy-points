using AutoMapper;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Domain.Entities;

namespace POSPRA.Application.AutoMapperProfile
{
    public class PosProfile : Profile
    {
        public PosProfile()
        {
            CreateMap<FileRecordDto, FileRecord>();
            CreateMap<FileRecord, FileRecordDto>();

            CreateMap<CreateLogDto, Logs>();
            CreateMap<Logs, CreateLogDto>();

            CreateMap<LogDto, Logs>();
            CreateMap<Logs, LogDto>();

            CreateMap<WorkerLogDto, Logs>();
            CreateMap<Logs, WorkerLogDto>();

            CreateMap<HeartBeatDto, PosClients>();
            CreateMap<PosClients, HeartBeatDto>();

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
                .ForMember(dest => dest.Items,
                           opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.BuyerNTN,
                           opt => opt.MapFrom(src => src.BuyerPNTN));

            // Child mapping
            CreateMap<InvoiceItemDto, InvoiceItems>()
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            //CreateMap<SyncLogDto, Logs>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 0));
        }
    }
}
