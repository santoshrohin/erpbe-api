using Microsoft.AspNetCore.Mvc;
using MediatR;
using ErpBE.API.Common;
using ErpBE.Application.Logs.Queries;
using ErpBE.Application.Logs.Commands;
using ErpBE.Application.Logs.DTOs;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can view logs
    public class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get application logs with filtering and pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50)</param>
        /// <param name="level">Log level filter (Information, Warning, Error, etc.)</param>
        /// <param name="searchTerm">Search term for message content</param>
        /// <param name="startDate">Start date filter</param>
        /// <param name="endDate">End date filter</param>
        /// <returns>Paginated list of logs</returns>
        [HttpGet]
        public async Task<IActionResult> GetLogs(
            int pageNumber = 1,
            int pageSize = 50,
            string? level = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = new GetLogsQuery
                {
                    QueryParameters = new LogsQueryParameters
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Level = level,
                        SearchTerm = searchTerm,
                        StartDate = startDate,
                        EndDate = endDate
                    }
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving logs", error = ex.Message });
            }
        }

        /// <summary>
        /// Get log statistics
        /// </summary>
        /// <returns>Log statistics by level and date</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetLogStatistics()
        {
            try
            {
                var query = new GetLogStatisticsQuery();
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving log statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Clear old logs (older than specified days)
        /// </summary>
        /// <param name="daysToKeep">Number of days to keep logs (default: 30)</param>
        /// <returns>Number of logs deleted</returns>
        [HttpDelete("cleanup")]
        [AuthorizeAdmin] // Only admins can clean up logs
        public async Task<IActionResult> CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                var command = new CleanupOldLogsCommand { DaysToKeep = daysToKeep };
                var deletedCount = await _mediator.Send(command);
                return Ok(new { message = $"Deleted {deletedCount} old log entries", deletedCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error cleaning up logs", error = ex.Message });
            }
        }
    }
}
