using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Interfaces;
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
        public async Task<ApiResponse<List<FileRecordDto>>> GetAllAsync()
        {
            var output = await _sqliteFileRecordRepository.GetAllAsync();
            var fileRecrodDTO = _mapper.Map<List<FileRecordDto>>(output);

            return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, fileRecrodDTO, string.Empty);
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

            return new ApiResponse<List<FileRecordDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
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