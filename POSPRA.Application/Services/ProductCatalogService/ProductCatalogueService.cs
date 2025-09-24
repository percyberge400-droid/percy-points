using AutoMapper;
using POSPRA.Application.Services.LogService;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA.Repositories.ProductCatalogueRepository;

namespace POSPRA.Application.Services.ProductCatalogService
{
    public class ProductCatalogueService : IProductCatalogueService
    {
        private readonly ILogService _logService;
        private readonly IProductCatalogueRepository _productCatalogueRepository;
        private readonly IMapper _mapper;

        public ProductCatalogueService(ILogService logService,
            IProductCatalogueRepository productCatalogueRepository,
            IMapper mapper)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _productCatalogueRepository = productCatalogueRepository ?? throw new ArgumentNullException(nameof(productCatalogueRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync()
        {
            var output = await _productCatalogueRepository.QueryProcedureAsync<ProductCatalogue>("GetProductCatalogue") ?? new List<ProductCatalogue>();
            var productCatalogueDTO = _mapper.Map<List<ProductCatalogueDto>>(output) ?? new List<ProductCatalogueDto>();

            return new ApiResponse<List<ProductCatalogueDto>>(null, null, productCatalogueDTO, null);
        }
    }
}
