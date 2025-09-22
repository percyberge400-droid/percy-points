using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.LiveService;
using POSPRA.DTOs.InvoiceDTOs;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveController : ControllerBase
    {
        private readonly ILiveService _liveService;
        public LiveController(ILiveService liveService)
        {
            _liveService = liveService;
        }

        [HttpPost("SaveData")]
        public async Task<IActionResult> Create([FromBody] List<FileRecordDTO> dto) =>
        Ok(await _liveService.SaveInvoicData(dto));
    }
}
