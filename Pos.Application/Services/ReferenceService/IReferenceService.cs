using Pos.Application.DTOs;
using Pos.Application.DTOs.ReferenceDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pos.Application.Services.ReferenceService
{
    public interface IReferenceService
    {
        Task<ApiResponse<List<ReferenceDto>>> GetAllPaymentsAsync(string environment);
        Task<ApiResponse<List<ReferenceDto>>> GetAllInvoiceTypesAsync(string environment);
        Task<ApiResponse<List<ReferenceDto>>> GetAllServicesRenderedAsync(string environment);
    }
}
