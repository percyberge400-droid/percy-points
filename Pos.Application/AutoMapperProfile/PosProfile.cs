using AutoMapper;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Domain.Entities;

namespace Pos.Application.AutoMapperProfile
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

            // Payment
            CreateMap<Payment, ReferenceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME ?? string.Empty));
            CreateMap<ReferenceDto, Payment>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NAME, opt => opt.MapFrom(src => src.Name ?? string.Empty));

            // InvoiceType
            CreateMap<InvoiceType, ReferenceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME ?? string.Empty));
            CreateMap<ReferenceDto, InvoiceType>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NAME, opt => opt.MapFrom(src => src.Name ?? string.Empty));

            // ServiceRendered
            CreateMap<ServiceRendered, ReferenceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME ?? string.Empty));
            CreateMap<ReferenceDto, ServiceRendered>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NAME, opt => opt.MapFrom(src => src.Name ?? string.Empty));

            // DTO ➜ Entity
            // Child mapping
            CreateMap<InvoiceItemDto, InvoiceItems>()
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            // Parent mapping
            CreateMap<InvoiceDto, Invoice>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.BuyerNTN, opt => opt.MapFrom(src => src.BuyerPNTN));
        }
    }
}
