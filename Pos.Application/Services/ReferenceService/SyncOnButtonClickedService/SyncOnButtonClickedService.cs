using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services.ReferenceService.InvoiceTypeService;
using Pos.Application.Services.ReferenceService.PaymentService;
using Pos.Application.Services.ReferenceService.ServicesRenderedService;

namespace Pos.Application.Services.ReferenceService.SyncOnButtonClickedService
{
    public class SyncOnButtonClickedService : ISyncOnButtonClickedService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInvoiceTypeRepository _invoiceTypeRepository;
        private readonly IServiceRenderedRepository _servicesRenderedRepository;

        private readonly IPaymentService _paymentService;
        private readonly IInvoiceTypeService _invoiceTypeService;
        private readonly IServicesRenderedService _servicesRenderedService;

        public SyncOnButtonClickedService(
            IPaymentRepository paymentRepository,
            IInvoiceTypeRepository invoiceTypeRepository,
            IServiceRenderedRepository servicesRenderedRepository,
            IPaymentService paymentService,
            IInvoiceTypeService invoiceTypeService,
            IServicesRenderedService servicesRenderedService)
        {
            _paymentRepository = paymentRepository;
            _invoiceTypeRepository = invoiceTypeRepository;
            _servicesRenderedRepository = servicesRenderedRepository;
            _paymentService = paymentService;
            _invoiceTypeService = invoiceTypeService;
            _servicesRenderedService = servicesRenderedService;
        }

        public async Task SyncAllReferenceDataAsync(string environment, string password)
        {
            await SyncPaymentsAsync(environment);
            await SyncInvoiceTypesAsync(environment);
            await SyncServicesRenderedAsync(environment);
        }

        private async Task SyncPaymentsAsync(string environment)
        {
            var entities = await _paymentRepository.GetAllPayment(environment);
            if (entities == null || !entities.Any())
                return;

            var dtos = entities.Select(x => new ReferenceDto
            {
                Id = (int)x.ID,
                Name = x.NAME ?? string.Empty
            }).ToList();

            await _paymentService.SyncPaymentMethodsAsync(environment, dtos);
        }

        private async Task SyncInvoiceTypesAsync(string environment)
        {
            var entities = await _invoiceTypeRepository.GetAllInvoiceType(environment);
            if (entities == null || !entities.Any())
                return;

            var dtos = entities.Select(x => new ReferenceDto
            {
                Id = (int)x.ID,
                Name = x.NAME ?? string.Empty
            }).ToList();

            await _invoiceTypeService.SyncInvoiceTypesAsync(environment, dtos);
        }

        private async Task SyncServicesRenderedAsync(string environment)
        {
            var entities = await _servicesRenderedRepository.GetServiceRendered(environment);
            if (entities == null || !entities.Any())
                return;

            var dtos = entities.Select(x => new ReferenceDto
            {
                Id = (int)x.ID,
                Name = x.NAME ?? string.Empty
            }).ToList();

            await _servicesRenderedService.SyncServicesRenderedAsync(environment, dtos);
        }
    }
}