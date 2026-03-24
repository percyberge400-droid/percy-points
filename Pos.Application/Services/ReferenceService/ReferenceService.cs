using AutoMapper;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pos.Application.Services.ReferenceService
{
    public class ReferenceService : IReferenceService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IInvoiceTypeRepository _invoiceRepo;
        private readonly IServiceRenderedRepository _serviceRenderedRepo;
        private readonly IMapper _mapper;

        public ReferenceService(
            IPaymentRepository paymentRepo,
            IInvoiceTypeRepository invoiceRepo,
            IServiceRenderedRepository serviceRenderedRepo,
            IMapper mapper)
        {
            _paymentRepo = paymentRepo;
            _invoiceRepo = invoiceRepo;
            _serviceRenderedRepo = serviceRenderedRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ReferenceDto>>> GetAllPaymentsAsync(string environment)
        {
            try
            {
                var result = await _paymentRepo.GetAllPayment(environment);
                return new ApiResponse<List<ReferenceDto>>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: _mapper.Map<List<ReferenceDto>>(result),
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

        public async Task<ApiResponse<List<ReferenceDto>>> GetAllInvoiceTypesAsync(string environment)
        {
            try
            {
                var result = await _invoiceRepo.GetAllInvoiceType(environment);
                return new ApiResponse<List<ReferenceDto>>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: _mapper.Map<List<ReferenceDto>>(result),
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

        public async Task<ApiResponse<List<ReferenceDto>>> GetAllServicesRenderedAsync(string environment)
        {
            try
            {
                var result = await _serviceRenderedRepo.GetServiceRendered(environment);
                return new ApiResponse<List<ReferenceDto>>(
                    statusCode: ApiStatusCode.Success,
                    message: ResponseMessages.RecordFound,
                    data: _mapper.Map<List<ReferenceDto>>(result),
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
    }
}