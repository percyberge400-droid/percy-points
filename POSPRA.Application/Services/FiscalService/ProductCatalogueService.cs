using AutoMapper;
using POSPRA.Application.Services.LogService;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.Repositories.ProductCatalogueRepository;

namespace POSPRA.Application.Services.FiscalService
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

        public async Task<ApiResponse<List<ProductCatalogueDTO>>> GetAllAsync()
        {
            var output = await _productCatalogueRepository.QueryProcedureAsync<ProductCatalogue>("GetProductCatalogue") ?? new List<ProductCatalogue>();
            var productCatalogueDTO = _mapper.Map<List<ProductCatalogueDTO>>(output) ?? new List<ProductCatalogueDTO>();

            return new ApiResponse<List<ProductCatalogueDTO>>(null, null, productCatalogueDTO, null);
        }
    }
}
