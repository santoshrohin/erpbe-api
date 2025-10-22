using Microsoft.AspNetCore.Mvc;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.DTOs;
using ErpBE.Application.UnitMaster.Commands;
using ErpBE.Application.UnitMaster.Queries;
using MediatR;
using ErpBE.Domain.CommonDto;

namespace ErpBE.API.Controllers.Test
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditTestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuditService _auditService;

        public AuditTestController(IMediator mediator, IAuditService auditService)
        {
            _mediator = mediator;
            _auditService = auditService;
        }

        /// <summary>
        /// Tests UnitMaster with audit trail support.
        /// </summary>
        [HttpGet("unit-master-with-audit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestUnitMasterWithAudit()
        {
            try
            {
                var queryParameters = new UnitMasterQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 5,
                    CompanyId = 1,
                    IsActive = true
                };

                var query = new GetUnitMastersQuery { QueryParameters = queryParameters };
                var units = await _mediator.Send(query);
                
                return Ok(new { 
                    message = "UnitMaster with audit trail test successful", 
                    totalCount = units.TotalCount,
                    dataCount = units.Data.Count,
                    data = units.Data,
                    hasAuditData = units.Data.Any(u => !string.IsNullOrEmpty(u.CreatedBy))
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Test failed", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }

        /// <summary>
        /// Tests creating a new UnitMaster and verifies audit trail.
        /// </summary>
        [HttpPost("create-unit-with-audit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestCreateUnitWithAudit()
        {
            try
            {
                var createRequest = new CreateUnitMasterRequest
                {
                    UnitName = $"TEST_UNIT_{DateTime.Now:HHmmss}",
                    UnitDescription = "Test Unit for Audit",
                    CompanyId = 1,
                    IsActive = true
                };

                var command = new CreateUnitMasterCommand { Request = createRequest };
                var unitId = await _mediator.Send(command);
                
                // Get audit trail for this unit
                var auditTrail = await _auditService.GetAuditTrailAsync("ITEM_UNIT_MASTER", unitId);
                
                return Ok(new { 
                    message = "Unit created with audit trail", 
                    unitId = unitId,
                    auditEntries = auditTrail.Count,
                    auditData = auditTrail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Test failed", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }

        /// <summary>
        /// Tests audit trail retrieval.
        /// </summary>
        [HttpGet("audit-trail/{tableName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestAuditTrail(string tableName, int? recordId = null)
        {
            try
            {
                var auditTrail = await _auditService.GetAuditTrailAsync(tableName, recordId, 1, 10);
                
                return Ok(new { 
                    message = "Audit trail retrieved successfully", 
                    tableName = tableName,
                    recordId = recordId,
                    count = auditTrail.Count,
                    data = auditTrail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Test failed", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }

        /// <summary>
        /// Tests getting a UnitMaster by ID with audit trail.
        /// </summary>
        [HttpGet("unit-master-by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestUnitMasterById(int id)
        {
            try
            {
                var query = new GetUnitMasterByIdQuery { Id = id };
                var unit = await _mediator.Send(query);
                
                return Ok(new { 
                    message = "UnitMaster by ID test successful", 
                    unit = unit,
                    hasAuditData = unit != null && !string.IsNullOrEmpty(unit.CreatedBy)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Test failed", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }

        /// <summary>
        /// Tests updating a unit and verifies audit trail.
        /// </summary>
        [HttpPut("update-unit-with-audit/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestUpdateUnitWithAudit(int unitId)
        {
            try
            {
                var updateRequest = new UpdateUnitMasterRequest
                {
                    Id = unitId,
                    UnitName = $"UPDATED_UNIT_{DateTime.Now:HHmmss}",
                    UnitDescription = "Updated Unit for Audit",
                    IsActive = true
                };

                var command = new UpdateUnitMasterCommand { Request = updateRequest };
                var updated = await _mediator.Send(command);
                
                // Get audit trail for this unit
                var auditTrail = await _auditService.GetAuditTrailAsync("ITEM_UNIT_MASTER", unitId);
                
                return Ok(new { 
                    message = "Unit updated with audit trail", 
                    unitId = unitId,
                    updated = updated,
                    auditEntries = auditTrail.Count,
                    auditData = auditTrail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Test failed", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }
}