using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pos.Application.Services.ReferenceService.LocalReferenceService
{
    public class LocalReferenceService<TEntity> : ILocalReferenceService<TEntity>
        where TEntity : class
    {
        private readonly IRepository<TEntity> _sqliteRepo;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IMapper _mapper;

        public LocalReferenceService(
            IRepository<TEntity> sqliteRepo,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IMapper mapper)
        {
            _sqliteRepo = sqliteRepo;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper;
        }

        public async Task SyncAsync(string env, IEnumerable<ReferenceDto> serverDtos)
        {
            if (serverDtos == null || !serverDtos.Any())
                return;

            var existing = await _sqliteRepo.GetAllAsync();
            if (existing.Any())
            {
                _sqliteRepo.RemoveRange(existing);
            }

            var entities = _mapper.Map<List<TEntity>>(serverDtos);
            await _sqliteRepo.AddRangeAsync(entities);
            await _sqliteUnitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<List<ReferenceDto>>> GetAllAsync()
        {
            try
            {
                var local = await _sqliteRepo.GetAllAsync();
                var result = _mapper.Map<List<ReferenceDto>>(local);

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
                    message: ResponseMessages.UnknownError,
                    data: null!,
                    errors: new { Exception = ex.Message }
                );
            }
        }

        public async Task<ApiResponse<int>> GetCountAsync()
        {
            try
            {
                var local = await _sqliteRepo.GetAllAsync();

                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: local.Count(),
                    errors: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.UnknownError,
                    data: 0,
                    errors: new { Exception = ex.Message }
                );
            }
        }
    }
}
