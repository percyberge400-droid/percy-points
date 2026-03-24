using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ReferenceService.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _sqliteFileRecordRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(
            IRepository<Payment> sqliteFileRecordRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IMapper mapper)
        {
            _sqliteFileRecordRepository = sqliteFileRecordRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper;
        }

        public async Task SyncPaymentMethodsAsync(string env, IEnumerable<ReferenceDto> serverPayments)
        {
            if (serverPayments == null || !serverPayments.Any())
                return;

            var existing = await _sqliteFileRecordRepository.GetAllAsync();
            if (existing.Any())
            {
                _sqliteFileRecordRepository.RemoveRange(existing);
            }

            var paymentsToAdd = _mapper.Map<List<Payment>>(serverPayments);
            await _sqliteFileRecordRepository.AddRangeAsync(paymentsToAdd);
            await _sqliteUnitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<List<ReferenceDto>>> GetPaymentMethodsAsync()
        {
            try
            {
                var localPayments = await _sqliteFileRecordRepository.GetAllAsync();
                var result = _mapper.Map<List<ReferenceDto>>(localPayments);

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
                    message: ResponseMessages.ErrorGettingPaymentMethods,
                    data: null!,
                    errors: new { Exception = ex.Message }
                );
            }
        }

        public async Task<ApiResponse<int>> GetPaymentMethodsCountAsync()
        {
            try
            {
                var localPayments = await _sqliteFileRecordRepository.GetAllAsync();

                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: localPayments.Count(),
                    errors: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.ErrorGettingPaymentMethodsCount,
                    data: 0,
                    errors: new { Exception = ex.Message }
                );
            }
        }
    }
}