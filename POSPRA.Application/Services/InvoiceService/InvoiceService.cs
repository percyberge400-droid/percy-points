using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA.Repositories.FiscalRepository;
using System.Text.Json;

namespace POSPRA.Application.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        public readonly IMapper _mapper;
        private readonly IFiscalRepository _fileRecordRepository;
        private readonly AppSettings _settings;

        public InvoiceService(IMapper mapper,
            IFiscalRepository fileRecordRepository,
            IOptions<AppSettings> options)
        {
            _mapper = mapper;
            _settings = options.Value;
            _fileRecordRepository = fileRecordRepository;
        }

        public async Task<ApiResponse<InvoiceDto>> GetInvoiceWithItems(string invoiceNumber)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            try
            {
                var output = await _fileRecordRepository.FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber);
                if (output != null)
                {
                    var decrypted = ModernAESEncryption.Decrypt(output.InvoiceData!, _settings.EC);
                    var jsonPart = decrypted.Split('|')[0];
                    if (string.IsNullOrWhiteSpace(jsonPart) ||
                        JsonSerializer.Deserialize<InvoiceDto>(jsonPart, options) is not { } invoiceDto)
                        return new ApiResponse<InvoiceDto>(
                            statusCode: ApiStatusCode.Error,
                            message: ResponseMessages.DataNotFound,
                            data: null!
                        );

                    return new ApiResponse<InvoiceDto>(
                        statusCode: ApiStatusCode.Success,
                        message: ResponseMessages.RecordFound,
                        data: invoiceDto
                    );
                }

                return new ApiResponse<InvoiceDto>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.DataNotFound,
                    data: null!
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<InvoiceDto>(
                    statusCode: ApiStatusCode.Error,
                    message: ResponseMessages.UnknownError,
                    data: null!
                );
            }
        }
    }
}