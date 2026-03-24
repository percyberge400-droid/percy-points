using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ReferenceService.InvoiceTypeService
{
    public class InvoiceTypeService : IInvoiceTypeService
    {
        private readonly IInvoiceTypeRepository _repo;
        private readonly IRepository<InvoiceType> _sqliteFileRecordRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IMapper _mapper;

        public InvoiceTypeService(
            IInvoiceTypeRepository repo,
            IRepository<InvoiceType> sqliteFileRecordRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IMapper mapper)
        {
            _repo = repo;
            _sqliteFileRecordRepository = sqliteFileRecordRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper;
        }

        public async Task SyncInvoiceTypesAsync(string env, IEnumerable<ReferenceDto> serverInvoiceTypes)
        {
            if (serverInvoiceTypes == null || !serverInvoiceTypes.Any())
                return;

            var existing = await _sqliteFileRecordRepository.GetAllAsync();
            if (existing.Any())
            {
                _sqliteFileRecordRepository.RemoveRange(existing);
            }

            var invoiceTypesToAdd = _mapper.Map<List<InvoiceType>>(serverInvoiceTypes);
            await _sqliteFileRecordRepository.AddRangeAsync(invoiceTypesToAdd);
            await _sqliteUnitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<List<ReferenceDto>>> GetInvoiceTypesAsync()
        {
            try
            {
                var localInvoiceTypes = await _sqliteFileRecordRepository.GetAllAsync();
                var result = _mapper.Map<List<ReferenceDto>>(localInvoiceTypes);

                return new ApiResponse<List<ReferenceDto>>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: result,
                    errors: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ReferenceDto>>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.ErrorGettingInvoiceTypes,
                    data: null!,
                    errors: new { Exception = ex.Message }
                );
            }
        }

        public async Task<ApiResponse<int>> GetInvoiceTypesCountAsync()
        {
            try
            {
                var localInvoiceTypes = await _sqliteFileRecordRepository.GetAllAsync();

                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: localInvoiceTypes.Count(),
                    errors: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.ErrorGettingInvoiceTypesCount,
                    data: 0,
                    errors: new { Exception = ex.Message }
                );
            }
        }
    }
}