using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Audit.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.UnitMaster.Commands;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Audit
{
    /// <summary>
    /// Integration tests for Audit functionality
    /// Tests audit log retrieval (matches reference implementation - tests handlers directly)
    /// </summary>
    public class AuditControllerTests : IntegrationTestBase
    {

        [Fact]
        public async Task GetAuditTrail_WithValidTableName_ShouldReturnOk()
        {
            // First create a unit master to generate audit trail
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 5);
            var createCommand = new CreateUnitMasterCommand
            {
                Request = new ErpBE.Application.DTOs.CreateUnitMasterRequest
                {
                    UnitName = $"AU{uniqueId}",
                    UnitDescription = "Audit Test Unit",
                    CompanyId = 1,
                    IsActive = true
                }
            };

            var unitId = await Mediator.Send(createCommand);
            unitId.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative

            try
            {
                // Now query audit trail
                var query = new GetAuditTrailQuery
                {
                    TableName = "ITEM_UNIT_MASTER",
                    PageNumber = 1,
                    PageSize = 50
                };

                var auditTrail = await Mediator.Send(query);

                // Assert
                auditTrail.Should().NotBeNull();
                auditTrail.Should().BeAssignableTo<List<AuditTrailDto>>();
            }
            finally
            {
                // Cleanup
                var deleteCommand = new DeleteUnitMasterCommand
                {
                    Id = unitId
                };
                await Mediator.Send(deleteCommand);
            }
        }

        [Fact]
        public async Task GetAuditTrail_WithPagination_ShouldReturnOk()
        {
            // Act
            var query = new GetAuditTrailQuery
            {
                TableName = "ITEM_UNIT_MASTER",
                PageNumber = 1,
                PageSize = 10
            };

            var auditTrail = await Mediator.Send(query);

            // Assert
            auditTrail.Should().NotBeNull();
            auditTrail.Should().BeAssignableTo<List<AuditTrailDto>>();
        }

        [Fact]
        public async Task GetRecordAuditTrail_WithValidRecord_ShouldReturnOk()
        {
            // Create a unit to get its audit trail
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 5);
            var createCommand = new CreateUnitMasterCommand
            {
                Request = new ErpBE.Application.DTOs.CreateUnitMasterRequest
                {
                    UnitName = $"AR{uniqueId}",
                    UnitDescription = "Record Audit Test",
                    CompanyId = 1,
                    IsActive = true
                }
            };

            var unitId = await Mediator.Send(createCommand);
            unitId.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative

            try
            {
                // Get audit trail for this specific record
                var query = new GetAuditTrailQuery
                {
                    TableName = "ITEM_UNIT_MASTER",
                    RecordId = unitId,
                    PageNumber = 1,
                    PageSize = 50
                };

                var auditTrail = await Mediator.Send(query);

                // Assert
                auditTrail.Should().NotBeNull();
                auditTrail.Should().BeAssignableTo<List<AuditTrailDto>>();
            }
            finally
            {
                // Cleanup
                var deleteCommand = new DeleteUnitMasterCommand
                {
                    Id = unitId
                };
                await Mediator.Send(deleteCommand);
            }
        }

        [Fact]
        public async Task GetAuditTrail_WithEmptyTableName_ShouldHandleGracefully()
        {
            // Act - Try with empty table name
            var query = new GetAuditTrailQuery
            {
                TableName = "",
                PageNumber = 1,
                PageSize = 50
            };

            // Assert - Should handle gracefully (either return empty list or throw exception)
            var auditTrail = await Mediator.Send(query);
            auditTrail.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAuditTrail_WithInvalidTableName_ShouldHandleGracefully()
        {
            // Act - Try with invalid table name
            var query = new GetAuditTrailQuery
            {
                TableName = "INVALID_TABLE_NAME_12345",
                PageNumber = 1,
                PageSize = 50
            };

            // Assert - Should handle gracefully (either return empty list or throw exception)
            var auditTrail = await Mediator.Send(query);
            auditTrail.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAuditTrail_WithNegativeRecordId_ShouldReturnOk()
        {
            // Act - Negative IDs are valid in this system
            var query = new GetAuditTrailQuery
            {
                TableName = "ITEM_UNIT_MASTER",
                RecordId = -999,
                PageNumber = 1,
                PageSize = 50
            };

            var auditTrail = await Mediator.Send(query);

            // Assert
            auditTrail.Should().NotBeNull();
            auditTrail.Should().BeAssignableTo<List<AuditTrailDto>>();
        }

        [Fact]
        public async Task GetRecordAuditTrail_WithLargePagination_ShouldReturnOk()
        {
            // Act - Test with maximum pagination (should be capped at 100)
            var query = new GetAuditTrailQuery
            {
                TableName = "ITEM_UNIT_MASTER",
                RecordId = 1,
                PageNumber = 1,
                PageSize = 200 // Should be capped at 100
            };

            var auditTrail = await Mediator.Send(query);

            // Assert
            auditTrail.Should().NotBeNull();
            auditTrail.Should().BeAssignableTo<List<AuditTrailDto>>();
        }
    }
}

