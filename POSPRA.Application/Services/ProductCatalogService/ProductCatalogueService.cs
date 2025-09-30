using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.ProductCatalogueRepository;

namespace POSPRA.Application.Services.ProductCatalogService
{
    public class ProductCatalogueService : IProductCatalogueService
    {
        private readonly SqlServerRepository<ProductCatalogue> _productCatalogueRepository;
        private readonly IMapper _mapper;

        public ProductCatalogueService(IProductCatalogueSQLServerRepository productCatalogueRepository, IMapper mapper,
            SqlServerRepository<ProductCatalogue> sqlServerRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _productCatalogueRepository = sqlServerRepository;
        }

        public async Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync(ProductCatalogueQueryDto dto)
        {
            try
            {
                IQueryable<ProductCatalogue> query = _productCatalogueRepository.Query();

                // Only filter HSCode if given
                if (!string.IsNullOrEmpty(dto.HSCode) && dto.HSCode is not null)
                    query = query.Where(i => i.HSCode!.Contains(dto.HSCode));

                // Only filter Product Description if given
                if (!string.IsNullOrEmpty(dto.ProductDescription) && dto.ProductDescription is not null)
                    query = query.Where(i => i.ProductDescription!.Contains(dto.ProductDescription));

                // Always order before pagination
                query = query.OrderBy(i => i.ProductCode);

                // Apply pagination
                int skip = (dto.pageNumber - 1) * dto.numberOfRecords;
                query = query.Skip(skip).Take(dto.numberOfRecords);

                var output = await query.ToListAsync();

                var productCatalogueDTO = _mapper.Map<List<ProductCatalogueDto>>(output);

                if (productCatalogueDTO.Any())
                    return new ApiResponse<List<ProductCatalogueDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, productCatalogueDTO, string.Empty);

                return new ApiResponse<List<ProductCatalogueDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
            }
            catch
            {
                throw;
            }
        }
    }
}
