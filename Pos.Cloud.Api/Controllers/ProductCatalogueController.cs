using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.Services.ProductCatalogService;

namespace Pos.Cloud.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCatalogueController(IProductCatalogueService productCatalogueService) : ControllerBase
    {
        private readonly IProductCatalogueService _productCatalogueService = productCatalogueService;

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(long posId) =>
            Ok(await _productCatalogueService.GetAllAsync(posId));

        /// <summary>
        /// This method is used to create product catalogue in SQLite.
        /// </summary>
        /// <param name="productCatalogueDto"></param>
        /// <returns></returns>
        [HttpPost("postproductcatalogue")]
        public async Task<IActionResult> Create([FromBody] ProductCatalogueDto productCatalogueDto) =>
            Ok(await _productCatalogueService.PostProductCatalog(productCatalogueDto));

        /// <summary>
        /// This API is used to get product catalog.
        /// </summary>
        /// <param name="productCatalogueDto"></param>
        /// <returns></returns>
        [HttpGet("getproductcatalogue")]
        public async Task<IActionResult> GetProductCatalogue() =>
            Ok(await _productCatalogueService.GetProductCatalogue());

        /// <summary>
        /// This API is used to delete product catalog.
        /// </summary>
        /// <param name="productCatalogueDto"></param>
        /// <returns></returns>
        [HttpGet("deleteproductcatalogue")]
        public async Task<IActionResult> DeleteProductCatalogue() =>
            Ok(await _productCatalogueService.DeleteProductCatalogue());
    }
}