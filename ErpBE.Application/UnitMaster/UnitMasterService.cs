using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.Common.Models;
using ErpBE.Application.Audit;
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
            // Validation is handled by FluentValidation in the pipeline
            var unitId = await _unitMasterRepository.CreateUnitMasterAsync(request);
            
            // Log audit trail
            await _auditService.LogCreateAsync("ITEM_UNIT_MASTER", unitId, request, "System");
            
            return unitId;
        }

        public async Task<bool> UpdateUnitMasterAsync(UpdateUnitMasterRequest request)
        {
            // Validation is handled by FluentValidation in the pipeline
            // Get existing unit for audit trail
            var existingUnit = await _unitMasterRepository.GetUnitMasterByIdAsync(request.Id);

            var updated = await _unitMasterRepository.UpdateUnitMasterAsync(request);
            
            if (updated && existingUnit != null)
            {
                // Log audit trail
                await _auditService.LogUpdateAsync("ITEM_UNIT_MASTER", request.Id, existingUnit, request, "System");
            }
            
            return updated;
        }

        public async Task<bool> DeleteUnitMasterAsync(int id)
        {
            // Validation is handled by FluentValidation in the pipeline
            // Get existing unit for audit trail
            var existingUnit = await _unitMasterRepository.GetUnitMasterByIdAsync(id);

            var deleted = await _unitMasterRepository.DeleteUnitMasterAsync(id);
            
            if (deleted && existingUnit != null)
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
