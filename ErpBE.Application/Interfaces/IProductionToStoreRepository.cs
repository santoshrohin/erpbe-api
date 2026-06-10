using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces
{
    public interface IProductionToStoreRepository
    {
        Task<(IEnumerable<ProductionToStoreMasterDto> Data, int TotalCount)> GetAllAsync(ProductionToStoreQueryParameters parameters);
        Task<ProductionToStoreMasterDto?>                                    GetByIdAsync(int productionCode, int companyCode);
        Task<ProductionToStoreMasterDto>                                     CreateAsync(CreateProductionToStoreRequest request);
        Task<ProductionToStoreMasterDto>                                     UpdateAsync(UpdateProductionToStoreRequest request);
        Task<bool>                                                            DeleteAsync(int productionCode, int companyCode);
    }
}
