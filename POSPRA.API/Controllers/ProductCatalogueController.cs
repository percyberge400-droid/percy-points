using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.DTOs.ProductCatalogDtos;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCatalogueController(IProductCatalogueService productCatalogueService) : ControllerBase
    {
        private readonly IProductCatalogueService _productCatalogueService = productCatalogueService;

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll(ProductCatalogueQueryDto dto) =>
            Ok(await _productCatalogueService.GetAllAsync(dto));

    }
}
