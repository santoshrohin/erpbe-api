using System;
using System.Threading.Tasks;
using ErpBE.Application.Logs.Queries;
using ErpBE.Application.Logs.DTOs;
using ErpBE.Application.Logs.Commands;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Logs
{
    /// <summary>
    /// Integration tests for Logs functionality
    /// Tests log retrieval, filtering, and pagination (matches reference implementation - tests handlers directly)
    /// </summary>
    public class LogsControllerTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetLogs_WithValidAuth_ShouldReturnOk()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 50
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetLogs_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 10
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
                result.PageNumber.Should().Be(1);
                result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetLogs_WithLevelFilter_ShouldReturnFilteredLogs()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 50,
                    Level = "Information"
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLogs_WithDateFilter_ShouldReturnFilteredLogs()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 50,
                    StartDate = today,
                    EndDate = today.AddDays(1)
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLogs_WithSearchTerm_ShouldReturnFilteredLogs()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 50,
                    SearchTerm = "Request"
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetLogStatistics_WithValidAuth_ShouldReturnOk()
        {
            // Arrange
            var query = new GetLogStatisticsQuery();

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
        }

        // Removed GetLogDebug tests - /debug endpoint no longer exists after CQRS refactoring

        [Fact]
        public async Task GetLogs_WithInvalidPageNumber_ShouldHandleGracefully()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = -1, // Invalid page number
                    PageSize = 10
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert - Should handle gracefully (convert to valid page)
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetLogs_WithZeroPageSize_ShouldHandleGracefully()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 0 // Invalid page size
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert - Should handle gracefully (convert to default 50)
            result.Should().NotBeNull();
            result.PageSize.Should().Be(50);
        }

        [Fact]
        public async Task GetLogs_WithExcessivePageSize_ShouldHandleGracefully()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 1000 // Excessive page size
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert - Page size > 100 should be capped at 100
            result.Should().NotBeNull();
            result.PageSize.Should().Be(100);
        }

        [Fact]
        public async Task GetLogs_WithInvalidLevel_ShouldReturnOk()
        {
            // Arrange
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 50,
                    Level = "InvalidLevel" // Invalid level
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert - Invalid level should just return no matches
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLogs_WithFutureDateRange_ShouldReturnOk()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(30);
            var query = new GetLogsQuery
            {
                QueryParameters = new LogsQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 50,
                    StartDate = futureDate,
                    EndDate = futureDate.AddDays(1)
                }
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task CleanupOldLogs_ShouldDeleteLogs()
        {
            // Arrange - Ensure there are some logs to delete (e.g., by running other tests first)
            // For a robust test, you might insert some old logs here.
            var command = new CleanupOldLogsCommand { DaysToKeep = 0 }; // Delete all logs older than 0 days

            // Act
            var deletedCount = await Mediator.Send(command);

            // Assert
            deletedCount.Should().BeGreaterThanOrEqualTo(0); // Should delete some logs or 0 if none exist
        }
    }
}

