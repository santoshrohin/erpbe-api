using Microsoft.AspNetCore.Mvc;
using ErpBE.API.Common;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.DTOs;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can view audit trails
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Gets audit trail for a specific table and optionally a specific record.
        /// </summary>
        /// <param name="tableName">Name of the table to get audit trail for.</param>
        /// <param name="recordId">Optional record ID to filter by.</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Page size for pagination.</param>
        /// <returns>List of audit trail entries.</returns>
        [HttpGet("{tableName}")]
        [ProducesResponseType(typeof(List<AuditTrailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAuditTrail(
            string tableName, 
            int? recordId = null, 
            int pageNumber = 1, 
            int pageSize = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return BadRequest(new { message = "Table name is required." });
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var auditTrail = await _auditService.GetAuditTrailAsync(tableName, recordId, pageNumber, pageSize);
                return Ok(auditTrail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving audit trail", detail = ex.Message });
            }
        }

        /// <summary>
        /// Gets audit trail for a specific record.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="recordId">Record ID to get audit trail for.</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Page size for pagination.</param>
        /// <returns>List of audit trail entries for the specific record.</returns>
        [HttpGet("{tableName}/{recordId}")]
        [ProducesResponseType(typeof(List<AuditTrailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecordAuditTrail(
            string tableName, 
            int recordId, 
            int pageNumber = 1, 
            int pageSize = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return BadRequest(new { message = "Table name is required." });
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var auditTrail = await _auditService.GetAuditTrailAsync(tableName, recordId, pageNumber, pageSize);
                return Ok(auditTrail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving record audit trail", detail = ex.Message });
            }
        }
    }
}
