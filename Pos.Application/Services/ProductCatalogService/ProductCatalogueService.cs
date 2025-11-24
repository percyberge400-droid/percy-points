using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.PageResponseDTOs;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
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
        private readonly IProductCatalogueByPosIdRepository _productCatalogueByPosIdRepository;

        private readonly AppSettings _settings;

        public ProductCatalogueService(
            IMapper mapper,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IConfiguration configuration,
            ISqlServerRepositoryFactory sqlRepositoryFactory,
            ISqliteRepositoryFactory sqliteRepositoryFactory,
            IProductCatalogueByPosIdRepository productCatalogueByPosIdRepository,
            IOptions<AppSettings> options)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _configuration = configuration;
            _settings = options.Value;
            _sqlProductCatalogueRepository = sqlRepositoryFactory.CreateRepository<ProductCatalogue>();
            _sqLiteProductCatalogueRepository = sqliteRepositoryFactory.CreateRepository<ProductCatalogue>();
            _productCatalogueByPosIdRepository = productCatalogueByPosIdRepository;
        }

        public async Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync(long posId)
        {
            try
            {
                var result = await _productCatalogueByPosIdRepository.GetProductCatalogueByPosIdAsync(posId);

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
        public async Task<ApiResponse<PageResponseDto<ProductCatalogueDto>>>
            GetProductCatalogueWithPagination(ProductCatalogueQueryDto query)
        {
            // Query all items
            var queryable = _sqLiteProductCatalogueRepository.Query();

            // Single query: get total count and paged items
            var result = await queryable
                .OrderBy(p => p.ProductCode) // make sure to order for Skip/Take
                .Skip((query.pageNumber - 1) * query.numberOfRecords)
                .Take(query.numberOfRecords)
                .Select(p => new { Item = p }) // projection
                .ToListAsync();

            // Total count
            int totalRecords = await queryable.CountAsync(); // separate call is still needed for total count

            // Mapping
            var updatedDtos = _mapper.Map<List<ProductCatalogueDto>>(result.Select(r => r.Item).ToList());

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalRecords / query.numberOfRecords);

            var responseDto = new PageResponseDto<ProductCatalogueDto>
            {
                Items = updatedDtos,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };

            if (updatedDtos.Any())
                return new ApiResponse<PageResponseDto<ProductCatalogueDto>>(
                    ApiStatusCode.Success,
                    ResponseMessages.RecordFound,
                    responseDto,
                    string.Empty
                );

            return new ApiResponse<PageResponseDto<ProductCatalogueDto>>(
                ApiStatusCode.NotFound,
                ResponseMessages.DataNotFound,
                new PageResponseDto<ProductCatalogueDto>
                {
                    Items = new List<ProductCatalogueDto>(),
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                },
                string.Empty
            );
        }

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