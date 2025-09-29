using AutoMapper;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
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
            var output = await _productCatalogueRepository.GetAllAsync();
            var productCatalogueDTO = _mapper.Map<List<ProductCatalogueDto>>(output);

            if (productCatalogueDTO.Any())
                return new ApiResponse<List<ProductCatalogueDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, productCatalogueDTO, string.Empty);

            return new ApiResponse<List<ProductCatalogueDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }
    }
}
