using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.CommonDto;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.UnitMaster
{
    public class UnitMasterService : IUnitMasterService
    {
        private readonly IUnitMasterRepository _unitMasterRepository;
        private readonly ILogger<UnitMasterService> _logger;
        private readonly IAuditService _auditService;

        public UnitMasterService(IUnitMasterRepository unitMasterRepository, ILogger<UnitMasterService> logger, IAuditService auditService)
        {
            _unitMasterRepository = unitMasterRepository;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<int> CreateUnitMasterAsync(CreateUnitMasterRequest request)
        {
            // Check if unit name is unique
            if (!await _unitMasterRepository.IsUnitNameUniqueAsync(request.UnitName, request.CompanyId))
            {
                throw new InvalidOperationException($"Unit with name '{request.UnitName}' already exists for this company.");
            }

            var unitId = await _unitMasterRepository.CreateUnitMasterAsync(request);
            
            // Log audit trail
            await _auditService.LogCreateAsync("ITEM_UNIT_MASTER", unitId, request, "System");
            
            return unitId;
        }

        public async Task<bool> UpdateUnitMasterAsync(UpdateUnitMasterRequest request)
        {
            // Get existing unit to check company ID
            var existingUnit = await _unitMasterRepository.GetUnitMasterByIdAsync(request.Id);
            if (existingUnit == null)
            {
                throw new KeyNotFoundException($"Unit with ID '{request.Id}' not found.");
            }

            // Check if unit name is unique (excluding current record)
            if (!await _unitMasterRepository.IsUnitNameUniqueAsync(request.UnitName, existingUnit.CompanyId, request.Id))
            {
                throw new InvalidOperationException($"Unit with name '{request.UnitName}' already exists for this company.");
            }

            var updated = await _unitMasterRepository.UpdateUnitMasterAsync(request);
            
            if (updated)
            {
                // Log audit trail
                await _auditService.LogUpdateAsync("ITEM_UNIT_MASTER", request.Id, existingUnit, request, "System");
            }
            
            return updated;
        }

        public async Task<bool> DeleteUnitMasterAsync(int id)
        {
            // Get existing unit for audit trail
            var existingUnit = await _unitMasterRepository.GetUnitMasterByIdAsync(id);
            if (existingUnit == null)
            {
                throw new KeyNotFoundException($"Unit with ID '{id}' not found.");
            }

            var deleted = await _unitMasterRepository.DeleteUnitMasterAsync(id);
            
            if (deleted)
            {
                // Log audit trail
                await _auditService.LogDeleteAsync("ITEM_UNIT_MASTER", id, existingUnit, "System");
            }
            
            return deleted;
        }

        public async Task<UnitMasterDto?> GetUnitMasterByIdAsync(int id)
        {
            return await _unitMasterRepository.GetUnitMasterByIdAsync(id);
        }

        public async Task<UnitMasterDto?> GetUnitMasterByNameAsync(string unitName, int companyId)
        {
            return await _unitMasterRepository.GetUnitMasterByNameAsync(unitName, companyId);
        }

        public async Task<PagedResponse<UnitMasterDto>> GetUnitMastersAsync(UnitMasterQueryParameters queryParameters)
        {
            return await _unitMasterRepository.GetUnitMastersAsync(queryParameters);
        }

        public async Task<bool> SetUnitMasterActiveStatusAsync(int id, bool isActive)
        {
            return await _unitMasterRepository.SetUnitMasterActiveStatusAsync(id, isActive);
        }
    }
}
