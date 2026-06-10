using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.TaxInvoice.Queries;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class GetTaxInvoiceCustomersHandler
        : IRequestHandler<GetTaxInvoiceCustomersQuery, List<TaxInvoiceCustomerDto>>
    {
        private readonly ITaxInvoiceRepository _repository;
        public GetTaxInvoiceCustomersHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<List<TaxInvoiceCustomerDto>> Handle(GetTaxInvoiceCustomersQuery request, CancellationToken ct)
            => _repository.GetCustomersWithActivePosAsync(request.CompanyCode);
    }

    public class GetTaxInvoiceItemsByCustomerHandler
        : IRequestHandler<GetTaxInvoiceItemsByCustomerQuery, List<TaxInvoiceItemDto>>
    {
        private readonly ITaxInvoiceRepository _repository;
        public GetTaxInvoiceItemsByCustomerHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<List<TaxInvoiceItemDto>> Handle(GetTaxInvoiceItemsByCustomerQuery request, CancellationToken ct)
            => _repository.GetItemsByCustomerAsync(request.CustomerCode, request.CompanyCode);
    }

    public class GetTaxInvoiceItemDetailsHandler
        : IRequestHandler<GetTaxInvoiceItemDetailsQuery, TaxInvoiceItemDetailsDto?>
    {
        private readonly ITaxInvoiceRepository _repository;
        public GetTaxInvoiceItemDetailsHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<TaxInvoiceItemDetailsDto?> Handle(GetTaxInvoiceItemDetailsQuery request, CancellationToken ct)
            => _repository.GetItemDetailsAsync(request.ItemCode, request.CompanyCode);
    }

    public class GetTaxInvoicePOsHandler
        : IRequestHandler<GetTaxInvoicePOsQuery, List<TaxInvoicePoDto>>
    {
        private readonly ITaxInvoiceRepository _repository;
        public GetTaxInvoicePOsHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<List<TaxInvoicePoDto>> Handle(GetTaxInvoicePOsQuery request, CancellationToken ct)
            => _repository.GetPOsByItemCustomerAsync(request.ItemCode, request.CustomerCode, request.CompanyCode, request.InvoiceCode);
    }

    public class GetCompanyStateHandler
        : IRequestHandler<GetCompanyStateQuery, CompanyStateDto?>
    {
        private readonly ITaxInvoiceRepository _repository;
        public GetCompanyStateHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<CompanyStateDto?> Handle(GetCompanyStateQuery request, CancellationToken ct)
            => _repository.GetCompanyStateAsync(request.CompanyCode);
    }

    public class GetSalesTaxMasterHandler
        : IRequestHandler<GetSalesTaxMasterQuery, List<SalesTaxDto>>
    {
        private readonly ITaxInvoiceRepository _repository;
        public GetSalesTaxMasterHandler(ITaxInvoiceRepository repository) => _repository = repository;
        public Task<List<SalesTaxDto>> Handle(GetSalesTaxMasterQuery request, CancellationToken ct)
            => _repository.GetSalesTaxMasterAsync(request.CompanyCode);
    }
}
