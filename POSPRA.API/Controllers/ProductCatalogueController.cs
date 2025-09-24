using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.ProductCatalogService;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCatalogueController(IProductCatalogueService productCatalogueService) : ControllerBase
    {
        private readonly IProductCatalogueService _productCatalogueService = productCatalogueService;

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _productCatalogueService.GetAllAsync());

    }
}
