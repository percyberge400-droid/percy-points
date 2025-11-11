using AutoMapper;
using Microsoft.Extensions.Configuration;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.Interfaces;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ProductCatalogService
{
    public class ProductCatalogueService : IProductCatalogueService
    {
        private readonly IRepository<ProductCatalogue> _sqlProductCatalogueRepository;
        private readonly IRepository<ProductCatalogue> _sqLiteProductCatalogueRepository;

        private readonly IMapper _mapper;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IConfiguration _configuration;

        public ProductCatalogueService(
            IMapper mapper,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IConfiguration configuration,
            ISqlServerRepositoryFactory sqlRepositoryFactory,
            ISqliteRepositoryFactory sqliteRepositoryFactory
            )
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _configuration = configuration;

            _sqlProductCatalogueRepository = sqlRepositoryFactory.CreateRepository<ProductCatalogue>();
            _sqLiteProductCatalogueRepository = sqliteRepositoryFactory.CreateRepository<ProductCatalogue>();

        }

        public async Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync()
        {
            try
            {
                var result = await _sqlProductCatalogueRepository.GetAllAsync();
                //// Only filter HSCode if given
                //if (!string.IsNullOrEmpty(dto.HSCode) && dto.HSCode is not null)
                //    query = query.Where(i => i.HSCode!.Contains(dto.HSCode));

                //// Only filter Product Description if given
                //if (!string.IsNullOrEmpty(dto.ProductDescription) && dto.ProductDescription is not null)
                //    query = query.Where(i => i.ProductDescription!.Contains(dto.ProductDescription));

                //// Always order before pagination
                //query = query.OrderBy(i => i.ProductCode);

                //// Apply pagination
                //int skip = (dto.pageNumber - 1) * dto.numberOfRecords;
                //query = query.Skip(skip).Take(dto.numberOfRecords);

                //var output = await query.ToListAsync();

                var productCatalogueDTO = _mapper.Map<List<ProductCatalogueDto>>(result);

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
                await _sqLiteProductCatalogueRepository.AddAsync(entities);
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
            var output = await _sqLiteProductCatalogueRepository.GetAllAsync();
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

            var entities = await _sqLiteProductCatalogueRepository.GetAllAsync();

            if (entities.Any())
            {
                _sqLiteProductCatalogueRepository.RemoveRange(entities);
                await _sqliteUnitOfWork.SaveChangesAsync();

                return new ApiResponse<object>(ApiStatusCode.Success, ResponseMessages.RecordDeleted, null, string.Empty);
            }

            return new ApiResponse<object>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }
    }
}