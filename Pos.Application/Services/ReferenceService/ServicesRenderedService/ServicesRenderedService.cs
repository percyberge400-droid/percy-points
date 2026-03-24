using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ReferenceService.ServicesRenderedService
{
    public class ServicesRenderedService : IServicesRenderedService
    {
        private readonly IRepository<ServiceRendered> _sqliteFileRecordRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IMapper _mapper;

        public ServicesRenderedService(
            IRepository<ServiceRendered> sqliteFileRecordRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IMapper mapper)
        {
            _sqliteFileRecordRepository = sqliteFileRecordRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper;
        }

        public async Task SyncServicesRenderedAsync(string env, IEnumerable<ReferenceDto> serverServices)
        {
            if (serverServices == null || !serverServices.Any())
                return;

            var existing = await _sqliteFileRecordRepository.GetAllAsync();
            if (existing.Any())
            {
                _sqliteFileRecordRepository.RemoveRange(existing);
            }

            var servicesToAdd = _mapper.Map<List<ServiceRendered>>(serverServices);
            await _sqliteFileRecordRepository.AddRangeAsync(servicesToAdd);
            await _sqliteUnitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<List<ReferenceDto>>> GetServicesRenderedAsync()
        {
            try
            {
                var localServices = await _sqliteFileRecordRepository.GetAllAsync();
                var result = _mapper.Map<List<ReferenceDto>>(localServices);

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
                    message: ResponseMessages.ErrorGettingServicesRendered,
                    data: null!,
                    errors: new { Exception = ex.Message }
                );
            }
        }

        public async Task<ApiResponse<int>> GetServicesRenderedCountAsync()
        {
            try
            {
                var localServices = await _sqliteFileRecordRepository.GetAllAsync();

                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: localServices.Count(),
                    errors: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.ErrorGettingServicesRenderedCount,
                    data: 0,
                    errors: new { Exception = ex.Message }
                );
            }
        }
    }
}