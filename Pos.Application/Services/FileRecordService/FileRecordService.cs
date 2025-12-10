using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.PageResponseDTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Application.Services.FileRecordService
{
    public class FileRecordService : IFileRecordService
    {
        private readonly IRepository<FileRecord> _sqliteFileRecordRepository;
        private readonly IMapper _mapper;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly ILogService _logService;
        public FileRecordService(
            ISqliteRepositoryFactory sqliteRepositoryFactory,
            IMapper mapper,
            ISqliteUnitOfWork sqliteUnitOfWork,
            ILogService logService)
        {
            _sqliteFileRecordRepository = sqliteRepositoryFactory.CreateRepository<FileRecord>();
            _mapper = mapper;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _logService = logService;
        }
        /// <summary>
        /// Retrieves **all** file records from the repository.
        /// </summary>
        /// <remarks>
        /// This method fetches every record regardless of sync status,
        /// maps them to <see cref="FileRecordDTO"/>, and returns the list
        /// wrapped in an <see cref="ApiResponse{T}"/>.
        /// </remarks>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of all
        /// <see cref="FileRecordDTO"/> objects.
        /// </returns>
        public async Task<ApiResponse<PageResponseDto<FileRecordDto>>> GetAllAsync(GetAllFileRecordDto dto)
        {
            int pageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            int numberOfRecords = dto.NumberOfRecords <= 0 ? 10 : dto.NumberOfRecords;

            var query = _sqliteFileRecordRepository.Query();

            if (dto.StartDate.HasValue && dto.EndDate.HasValue)
            {
                DateTime start = dto.StartDate.Value.Date;
                DateTime end = dto.EndDate.Value.Date;
                query = query.Where(m => m.DateCreated.Date >= start && m.DateCreated.Date <= end);
            }

            // Total Records
            int totalRecords = await query.CountAsync();

            // Total Synced Invoices
            int totalSyncedInvoices = await query.CountAsync(m => m.IsSynced == 1);

            // Total Unsynced Invoices
            int unsyncedInvoices = await query.CountAsync(m => m.IsSynced == 0);

            // Check if no records exist
            if (totalRecords == 0)
            {
                return new ApiResponse<PageResponseDto<FileRecordDto>>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.RecordNotFound,
                    new PageResponseDto<FileRecordDto>
                    {
                        Items = new List<FileRecordDto>(),
                        TotalRecords = 0,
                        TotalPages = 0,
                        TotalSyncedInvoices = 0,
                        UnsyncedInvoices = 0
                    },
                    null!
                );
            }

            // Calculate Total Pages
            int totalPages = (int)Math.Ceiling((double)totalRecords / numberOfRecords);

            // Validate page number
            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            // Sort by DateCreated DESC (most recent first), then by ID DESC as tiebreaker
            query = query.OrderByDescending(m => m.DateCreated).ThenByDescending(m => m.ID);

            // Apply Pagination
            int skip = (pageNumber - 1) * numberOfRecords;
            var output = await query.Skip(skip).Take(numberOfRecords).ToListAsync();

            // Map to DTO
            var fileRecordDTO = _mapper.Map<List<FileRecordDto>>(output);

            // Return in a Paged Result
            var pagedResult = new PageResponseDto<FileRecordDto>
            {
                Items = fileRecordDTO ?? new List<FileRecordDto>(),
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                TotalSyncedInvoices = totalSyncedInvoices,
                UnsyncedInvoices = unsyncedInvoices
            };

            return new ApiResponse<PageResponseDto<FileRecordDto>>(
                ApiStatusCode.Success,
                ResponseMessages.RecordFound,
                pagedResult,
                string.Empty
            );
        }
        /// <summary>
        /// Retrieves only the file records that are **not yet synced**.
        /// </summary>
        /// <remarks>
        /// The method first obtains all records from the repository, filters them
        /// in memory to include only those whose <c>IsSynced</c> property equals
        /// <see cref="InvoiceStatus.NotSynced"/>, then maps the result to
        /// <see cref="FileRecordDTO"/> objects.
        /// </remarks>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of unsynced
        /// <see cref="FileRecordDTO"/> objects.
        /// </returns>
        public async Task<ApiResponse<List<FileRecordDto>>> GetAllUnsyncedAsync()
        {
            // Await the repository call directly (don't use .Result)
            var allRecords = await _sqliteFileRecordRepository.GetAllAsync();

            // Filter in memory for unsynced records
            var unsynced = allRecords
                .Where(x => x.IsSynced == (int)InvoiceStatus.NotSynced)
                .Take(1000)
                .ToList();

            // Map to DTOs
            var fileRecordDtos = _mapper.Map<List<FileRecordDto>>(unsynced);
            if (fileRecordDtos.Any())
                return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, fileRecordDtos, string.Empty);

            return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.NotFound, ResponseMessages.UnsyncedDataNotFound, null, string.Empty);
        }

        public async Task<ApiResponse<FileRecordDto>> GetByInvoiceIdAsync(int invoiceId)
        {
            var output = await _sqliteFileRecordRepository.GetByIdAsync(invoiceId);
            var fileRecrodDTO = _mapper.Map<FileRecordDto>(output);

            return new ApiResponse<FileRecordDto>(ApiStatusCode.Success, ResponseMessages.RecordFound, fileRecrodDTO, string.Empty);
        }

        /// <summary>
        /// Inserts the encrypted invoice data into the database as a FileRecord.
        /// </summary>
        /// <param name="posId">The POS identifier for the invoice.</param>
        /// <param name="encryptedData">The encrypted invoice data.</param>
        /// <param name="invoiceNumber">The generated invoice number.</param>
        /// <returns>
        /// The ID of the newly created FileRecord if successful; otherwise, 0.
        /// </returns>
        public async Task<int> CreateAsync(long posId, string encryptedData, string invoiceNumber)
        {
            try
            {
                var model = new FileRecord
                {
                    POSID = posId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceData = encryptedData,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    IsSynced = (int)InvoiceStatus.NotSynced,
                    AttemptCount = 0
                };

                await _sqliteFileRecordRepository.AddAsync(model);
                await _sqliteUnitOfWork.SaveChangesAsync();
                return model.ID;
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"{GlobalVariables.DATE} InsertInvoiceAsync failed: {ex.InnerException?.Message ?? ex.Message}";

                await _logService.CreateLogAsync(new CreateLogDto(errorMessage, AlertType.Exception, false));
                return 0;
            }
        }

        /// <summary>
        /// Updates multiple <see cref="FileRecordDto"/> objects in the database.
        /// </summary>
        /// <param name="fileRecordDtos">
        /// A list of <see cref="FileRecordDto"/> items to update.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a list of updated <see cref="FileRecordDto"/> 
        /// objects when successful, or an error response if validation fails or no records exist.
        /// </returns>
        public async Task<ApiResponse<FileRecordDto>> UpdateFileRecordAsync(FileRecordDto fileRecordDto)
        {
            if (fileRecordDto == null)
            {
                return new ApiResponse<FileRecordDto>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);
            }

            var entity = _mapper.Map<FileRecord>(fileRecordDto);
            await _sqliteFileRecordRepository.UpdateAsync(entity);
            await _sqliteUnitOfWork.SaveChangesAsync();

            var updatedDto = _mapper.Map<FileRecordDto>(entity);

            return new ApiResponse<FileRecordDto>(
                ApiStatusCode.Success,
                ResponseMessages.RecordSaved,
                updatedDto,
                string.Empty);
        }

        public async Task<ApiResponse<List<FileRecordDto>>> UpdateFileRecordsAsync(List<FileRecordDto> fileRecordDtos)
        {
            if (fileRecordDtos == null || fileRecordDtos.Count == 0)
            {
                return new ApiResponse<List<FileRecordDto>>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);
            }

            var entities = _mapper.Map<List<FileRecord>>(fileRecordDtos);
            _sqliteFileRecordRepository.UpdateRange(entities);
            await _sqliteUnitOfWork.SaveChangesAsync();

            var updatedDtos = _mapper.Map<List<FileRecordDto>>(entities);

            return new ApiResponse<List<FileRecordDto>>(
                ApiStatusCode.Success,
                ResponseMessages.RecordSaved,
                updatedDtos,
                string.Empty);
        }
    }
}