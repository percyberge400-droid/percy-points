using Microsoft.AspNetCore.Mvc;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IEnvironmentService _environmentService;

        public CustomerController(ICustomerService customerService, IEnvironmentService environmentService)
        {
            _customerService = customerService;
            _environmentService = environmentService;
        }

        // GET api/Customer/sql/{id}
        [HttpGet("sql/{id:long}")]
        public async Task<IActionResult> GetCustomerFromSql(long id)
        {
            try
            {
                PosClients? customer = await _customerService.GetByIdFromSqlAsync(id);
                var env = _environmentService.GetCurrentEnvironment();

                return Ok(new
                {
                    Environment = env.ToString(),
                    Source = "SQL Server",
                    Customer = customer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching the customer from SQL Server.",
                    Details = ex.Message
                });
            }
        }

        // GET api/Customer/sqlite/{id}
        [HttpGet("sqlite/{id:long}")]
        public async Task<IActionResult> GetCustomerFromSqlite(long id)
        {
            try
            {
                FileRecord? customer = await _customerService.GetByIdFromSqliteAsync(id);
                var env = _environmentService.GetCurrentEnvironment();

                return Ok(new
                {
                    Environment = env.ToString(),
                    Source = "SQLite",
                    Customer = customer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching the customer from SQLite.",
                    Details = ex.Message
                });
            }
        }
    }
}
