using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.LiveService
{
    public class LiveService : ILiveService
    {
        private readonly AppSettings _settings;
        private readonly SqlServerRepository<Invoice> _invoiceRepository;
        private readonly SqlServerRepository<InvoiceItems> _invoiceItemsRepository;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IFiscalRepository _fiscalRepository;

        public LiveService(
            IOptions<AppSettings> options,
            SqlServerRepository<Invoice> invoiceRepository,
            SqlServerRepository<InvoiceItems> invoiceItemsRepository,
            IMapper mapper,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IFiscalRepository fiscalRepository)
        {
            _settings = options.Value;          // ← this should have EC populated
            _invoiceRepository = invoiceRepository;
            _invoiceItemsRepository = invoiceItemsRepository;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _fiscalRepository = fiscalRepository;
        }


        public async Task<ApiResponse<FileRecordDTO>> SaveInvoicData(List<FileRecordDTO> dto)
        {
            if (!dto.Any())
                return new ApiResponse<FileRecordDTO>(
                    statusCode: ApiStatusCode.Error.ToString(),
                    message: ResponseMessages.DataNotFound,
                    data: null
                );

            try
            {
                await CreateDecryptedInvoice(dto);
            }
            catch (Exception ex)
            {
                return new ApiResponse<FileRecordDTO>(
                statusCode: ApiStatusCode.Error.ToString(),
                message: ResponseMessages.RecordSaved,
                data: null
                );
            }

            return new ApiResponse<FileRecordDTO>(
                    statusCode: ApiStatusCode.Success.ToString(),
                    message: ResponseMessages.RecordSaved,
                    data: null
            );
        }

        private async Task<ApiResponse<Invoice>> CreateDecryptedInvoice(List<FileRecordDTO> dTOs)
        {
            try
            {
                foreach (var item in dTOs)
                {
                    var decryptedInvoice = ModernAESEncryption.Decrypt(item.InvoiceData!, _settings.EC);

                    var parts = decryptedInvoice.Split('|');

                    // 2. The first part is the JSON
                    var jsonPart = parts[0];

                    // 3. Deserialize to your object
                    var invoiceDto = JsonSerializer.Deserialize<InvoiceDto>(jsonPart);

                    var Invoice = await SaveInvoiceWithItems(invoiceDto);

                    //if (!response.success)
                    //{
                    //    return new ApiResponse<Invoice>(
                    //        statusCode: ApiStatusCode.Success.ToString(),
                    //        message: ResponseMessages.RecordSaved,
                    //        data: null
                    //    );
                    //}
                    var fileRecord = await _fiscalRepository.FirstOrDefaultAsync(x => x.ID == item.ID && x.IsSynced == 0);
                    if (fileRecord != null)
                    {
                        fileRecord.IsSynced = 1;
                        await _sqliteUnitOfWork.SaveChangesAsync();
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return new ApiResponse<Invoice>(
                statusCode: ApiStatusCode.Success.ToString(),
                message: ResponseMessages.RecordSaved,
                data: null
            );
        }

        private async Task<ApiResponse<Invoice>> SaveInvoiceWithItems(InvoiceDto dto)
        {
            try
            {
                // 1️⃣ Save invoice first
                var invoice = new Invoice
                {
                    BPOSID = dto.BPOSID,
                    InvoiceType = dto.InvoiceType,
                    InvoiceDate = dto.InvoiceDate >= new DateTime(1753, 1, 1) ? dto.InvoiceDate : DateTime.Now,
                    NTN_CNIC = dto.NTN_CNIC,
                    BuyerSellerName = dto.BuyerSellerName,
                    DestinationAddress = dto.DestinationAddress,
                    SaleType = dto.SaleType,
                    TotalSalesTaxApplicable = dto.TotalSalesTaxApplicable,
                    TotalRetailPrice = dto.TotalRetailPrice,
                    TotalSTWithheldAtSource = dto.TotalSTWithheldAtSource,
                    TotalExtraTax = dto.TotalExtraTax,
                    TotalFEDPayable = dto.TotalFEDPayable,
                    TotalWithheldIncomeTax = dto.TotalWithheldIncomeTax,
                    TotalCVT = dto.TotalCVT,
                    Distributor_NTN_CNIC = dto.Distributor_NTN_CNIC,
                    DistributorName = dto.DistributorName,
                    //EntryDate = DateTime.Now,
                    IsActive = true
                };

                await _invoiceRepository.AddAsync(invoice);
                await _sqlServerUnitOfWork.SaveChangesAsync(); // ✅ After this, invoice.InvoiceID is populated

                if (dto.InvoiceItemDetails!.Any())
                {
                    var invoiceItemsList = dto.InvoiceItemDetails.Select(items => new InvoiceItems
                    {
                        InvoiceID = invoice.InvoiceID, // ← use the generated InvoiceID
                        HSCode = items.HSCode,
                        ProductCode = items.ProductCode,
                        ProductDescription = items.ProductDescription,
                        Rate = items.Rate,
                        UoM = items.UoM,
                        Quantity = items.Quantity,
                        ValueSalesExcludingST = items.ValueSalesExcludingST,
                        SalesTaxApplicable = items.SalesTaxApplicable,
                        RetailPrice = items.RetailPrice,
                        STWithheldAtSource = items.STWithheldAtSource,
                        ExtraTax = items.ExtraTax,
                        FurtherTax = items.FurtherTax,
                        SroScheduleNo = items.SroScheduleNo,
                        FedPayable = items.FedPayable,
                        CVT = items.CVT,
                        WHIT_1 = items.WHIT_1,
                        WHIT_2 = items.WHIT_2,
                        WHIT_Section_1 = items.WHIT_Section_1,
                        WHIT_Section_2 = items.WHIT_Section_2,
                        TotalValues = items.TotalValues,
                        EntryDate = DateTime.Now,
                        IsActive = true
                    }).ToList();

                    await _invoiceItemsRepository.AddRangeAsync(invoiceItemsList);
                    await _sqlServerUnitOfWork.SaveChangesAsync();
                }



                return new ApiResponse<Invoice>(
                    statusCode: ApiStatusCode.Success.ToString(),
                    message: ResponseMessages.RecordSaved,
                    data: invoice
                );
            }
            catch (Exception ex)
            {
                return new ApiResponse<Invoice>(
                    statusCode: ApiStatusCode.Error.ToString(),
                    message: ex.Message,
                    data: null
                );
            }
        }

    }
}
