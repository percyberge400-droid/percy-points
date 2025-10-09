using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.ProductCatalogueRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.ProductCatalogService
{
    public class ProductCatalogueService : IProductCatalogueService
    {
        private readonly SqlServerRepository<ProductCatalogue> _productCatalogueRepository;
        private readonly IMapper _mapper;
        private readonly IProductCatalogueSQLiteRepository _productCatalogueSQLiteRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;

        public ProductCatalogueService(IProductCatalogueSQLServerRepository productCatalogueRepository, IMapper mapper,
            SqlServerRepository<ProductCatalogue> sqlServerRepository, IProductCatalogueSQLiteRepository productCatalogueSQLiteRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _productCatalogueRepository = sqlServerRepository;
            _productCatalogueSQLiteRepository = productCatalogueSQLiteRepository;
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

        /// <summary>
        /// This method is used to create product catalogue in SQLite.
        /// </summary>
        /// <param name="productCatalogueDto"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ProductCatalogueDto>> PostProductCatalog(ProductCatalogueDto productCatalogueDto)
        {
            if (productCatalogueDto == null)
            {
                return new ApiResponse<ProductCatalogueDto>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);
            }

            var entities = _mapper.Map<ProductCatalogue>(productCatalogueDto);
            if (entities is not null)
            {
                await _productCatalogueSQLiteRepository.AddAsync(entities);
                await _sqliteUnitOfWork.SaveChangesAsync();
            }

            var updatedDtos = _mapper.Map<ProductCatalogueDto>(entities);

            return new ApiResponse<ProductCatalogueDto>(
                ApiStatusCode.Success,
                ResponseMessages.RecordSaved,
                updatedDtos,
                string.Empty);
        }
        /// <summary>
        /// This API is used to get product catalog.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<List<ProductCatalogueDto>>> GetProductCatalogue()
        {
            var output = await _productCatalogueSQLiteRepository.GetAllAsync();
            var updatedDtos = _mapper.Map<List<ProductCatalogueDto>>(output);

            if (updatedDtos.Any())
                return new ApiResponse<List<ProductCatalogueDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, updatedDtos, string.Empty);

            return new ApiResponse<List<ProductCatalogueDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }

        /// <summary>
        /// This API is used to delete product catalog.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<object>> DeleteProductCatalogue()
        {
            var entities = await _productCatalogueSQLiteRepository.GetAllAsync();

            if (entities.Any())
            {
                _productCatalogueSQLiteRepository.RemoveRange(entities);
                await _sqliteUnitOfWork.SaveChangesAsync();

                return new ApiResponse<object>(ApiStatusCode.Success, ResponseMessages.RecordDeleted, null, string.Empty);
            }

            return new ApiResponse<object>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }
    }
}
