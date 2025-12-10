using Microsoft.AspNetCore.Mvc;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.Services.FileRecordService;

namespace Pos.Local.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileRecordController(IFileRecordService fileRecordService) : ControllerBase
    {
        private readonly IFileRecordService _fileRecordService = fileRecordService;

        /// <summary>
        /// Retrieves all fiscal invoices from the database.
        /// </summary>
        /// <returns>
        /// A list of all invoices with their fiscal details.
        /// </returns>
        [HttpPost("getalls")]
        public async Task<IActionResult> GetAll(GetAllFileRecordDto dto) =>
            Ok(await _fileRecordService.GetAllAsync(dto));

        /// <summary>
        /// Retrieves all fiscal invoices that have not yet been synced.
        /// </summary>
        /// <returns>
        /// A list of unsynced invoices with their fiscal details.
        /// </returns>
        [HttpGet("getallunsynced")]
        public async Task<IActionResult> GetUnSyncedAll() =>
            Ok(await _fileRecordService.GetAllUnsyncedAsync());


        /// <summary>
        /// Retrieves all fiscal invoices that have not yet been synced.
        /// </summary>
        /// <returns>
        /// A list of unsynced invoices with their fiscal details.
        /// </returns>
        [HttpPost("update-filereacord")]
        public async Task<IActionResult> UpdateFileRecordAsync(List<FileRecordDto> dtos) =>
            Ok(await _fileRecordService.UpdateFileRecordsAsync(dtos));
    }
}
