using Microsoft.AspNetCore.Mvc;
using MediatR;
using ErpBE.API.Common;
using ErpBE.Application.Audit.Queries;
using ErpBE.Domain.DTOs;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can view audit trails
    public class AuditController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IMediator mediator, ILogger<AuditController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get audit trail for a table (optionally filtered by record ID)
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="recordId">Optional record ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of audit entries</returns>
        [HttpGet("{tableName}")]
        [ProducesResponseType(typeof(List<AuditTrailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditTrail(
            string tableName,
            [FromQuery] int? recordId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                // Validate and sanitize input
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var query = new GetAuditTrailQuery
                {
                    TableName = tableName,
                    RecordId = recordId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var auditTrail = await _mediator.Send(query);
                return Ok(auditTrail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit trail");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get audit trail for a specific record
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="recordId">Record ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of audit entries for the specific record</returns>
        [HttpGet("{tableName}/{recordId}")]
        [ProducesResponseType(typeof(List<AuditTrailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecordAuditTrail(
            string tableName,
            int recordId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                // Validate and sanitize input
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var query = new GetAuditTrailQuery
                {
                    TableName = tableName,
                    RecordId = recordId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var auditTrail = await _mediator.Send(query);
                return Ok(auditTrail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit trail");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
