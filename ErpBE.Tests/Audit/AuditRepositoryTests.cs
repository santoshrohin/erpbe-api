using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Audit.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Audit
{
    /// <summary>
    /// Integration tests for AuditRepository
    /// Tests audit entry creation, retrieval, and configuration management
    /// Tests use IntegrationTestBase to use test database (matches reference implementation)
    /// </summary>
    public class AuditRepositoryTests : IntegrationTestBase
    {
        private IAuditRepository _repository;
        private readonly List<int> _createdAuditIds = new();
        private readonly List<int> _createdConfigIds = new();

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = GetService<IAuditRepository>();
        }

        [Fact]
        public async Task CreateAsync_WithValidAuditEntry_ShouldReturnId()
        {
            // Arrange
            var auditEntry = new AuditEntry
            {
                EntityName = "TestEntity",
                EntityId = "123",
                Action = "CREATE",
                UserId = "TestUser",
                UserName = "Test User",
                UserRole = "Admin",
                CompanyId = "1",
                IpAddress = "127.0.0.1",
                UserAgent = "Test Agent",
                Endpoint = "/api/test",
                HttpMethod = "POST",
                Description = "Test audit entry",
                OldValues = null,
                NewValues = "{\"field\":\"value\"}",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var id = await _repository.CreateAsync(auditEntry);

            // Assert
            id.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative
            _createdAuditIds.Add(id);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnAuditEntry()
        {
            // Arrange - Create an audit entry first
            var auditEntry = new AuditEntry
            {
                EntityName = "TestEntity",
                EntityId = "456",
                Action = "UPDATE",
                UserId = "TestUser2",
                UserName = "Test User 2",
                UserRole = "Manager",
                CompanyId = "1",
                Timestamp = DateTime.UtcNow
            };
            var id = await _repository.CreateAsync(auditEntry);
            _createdAuditIds.Add(id);

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.EntityName.Should().Be("TestEntity");
            result.EntityId.Should().Be("456");
            result.Action.Should().Be("UPDATE");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999999999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPagedAsync_WithDefaultParameters_ShouldReturnPagedResults()
        {
            // Arrange - Create some test audit entries
            for (int i = 0; i < 3; i++)
            {
                var entry = new AuditEntry
                {
                    EntityName = "PagedTest",
                    EntityId = $"P{i}",
                    Action = "CREATE",
                    UserId = "PagedUser",
                    Timestamp = DateTime.UtcNow
                };
                var id = await _repository.CreateAsync(entry);
                _createdAuditIds.Add(id);
            }

            var parameters = new AuditQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                EntityName = "PagedTest"
            };

            // Act
            var result = await _repository.GetPagedAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.Data.Count.Should().BeGreaterThanOrEqualTo(3);
            result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetPagedAsync_WithEntityNameFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            var uniqueEntity = $"UniqueEntity_{Guid.NewGuid():N}";
            var entry = new AuditEntry
            {
                EntityName = uniqueEntity,
                EntityId = "1",
                Action = "CREATE",
                UserId = "FilterUser",
                Timestamp = DateTime.UtcNow
            };
            var id = await _repository.CreateAsync(entry);
            _createdAuditIds.Add(id);

            var parameters = new AuditQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                EntityName = uniqueEntity
            };

            // Act
            var result = await _repository.GetPagedAsync(parameters);

            // Assert
            result.Data.Should().NotBeEmpty();
            result.Data.Should().OnlyContain(x => x.EntityName == uniqueEntity);
        }

        [Fact]
        public async Task GetPagedAsync_WithActionFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N");
            var entry = new AuditEntry
            {
                EntityName = "ActionTest",
                EntityId = uniqueId,
                Action = "DELETE",
                UserId = "ActionUser",
                Timestamp = DateTime.UtcNow
            };
            var id = await _repository.CreateAsync(entry);
            _createdAuditIds.Add(id);

            var parameters = new AuditQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                EntityId = uniqueId,
                Action = "DELETE"
            };

            // Act
            var result = await _repository.GetPagedAsync(parameters);

            // Assert
            result.Data.Should().NotBeEmpty();
            result.Data.Should().OnlyContain(x => x.Action == "DELETE");
        }

        [Fact]
        public async Task GetPagedAsync_WithPagination_ShouldRespectPageSize()
        {
            // Arrange
            var parameters = new AuditQueryParameters
            {
                PageNumber = 1,
                PageSize = 5
            };

            // Act
            var result = await _repository.GetPagedAsync(parameters);

            // Assert
            result.Data.Count.Should().BeLessOrEqualTo(5);
        }

        [Fact]
        public async Task GetAuditHistoryAsync_WithValidEntityAndId_ShouldReturnHistory()
        {
            // Arrange - Create multiple audit entries for same entity
            var entityId = Guid.NewGuid().ToString();
            for (int i = 0; i < 3; i++)
            {
                var entry = new AuditEntry
                {
                    EntityName = "HistoryEntity",
                    EntityId = entityId,
                    Action = i == 0 ? "CREATE" : "UPDATE",
                    UserId = "HistoryUser",
                    Timestamp = DateTime.UtcNow.AddMinutes(-i)
                };
                var id = await _repository.CreateAsync(entry);
                _createdAuditIds.Add(id);
            }

            // Act
            var history = await _repository.GetAuditHistoryAsync("HistoryEntity", entityId);

            // Assert
            history.Should().NotBeEmpty();
            history.Count.Should().BeGreaterThanOrEqualTo(3);
            history.Should().OnlyContain(x => x.EntityName == "HistoryEntity" && x.EntityId == entityId);
        }

        [Fact]
        public async Task GetAuditHistoryAsync_WithNonExistingEntity_ShouldReturnEmptyList()
        {
            // Act
            var history = await _repository.GetAuditHistoryAsync("NonExistentEntity", "999999");

            // Assert
            history.Should().NotBeNull();
            history.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateConfigurationAsync_WithValidConfig_ShouldReturnId()
        {
            // Arrange
            var config = new AuditConfiguration
            {
                Endpoint = $"/api/test/config/{Guid.NewGuid():N}",
                HttpMethod = "POST",
                EntityName = "ConfigTest",
                EntityIdProperty = "Id",
                IsEnabled = true,
                TrackPropertyChanges = true,
                TrackOldValues = true,
                TrackNewValues = true,
                Description = "Test configuration",
                CreatedBy = "TestUser"
            };

            // Act
            var id = await _repository.CreateConfigurationAsync(config);

            // Assert
            id.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative
            _createdConfigIds.Add(id);
        }

        [Fact]
        public async Task GetAuditConfigurationAsync_WithExistingEndpoint_ShouldReturnConfig()
        {
            // Arrange
            var uniqueEndpoint = $"/api/test/{Guid.NewGuid():N}";
            var config = new AuditConfiguration
            {
                Endpoint = uniqueEndpoint,
                HttpMethod = "GET",
                EntityName = "GetConfigTest",
                IsEnabled = true,
                CreatedBy = "TestUser"
            };
            var id = await _repository.CreateConfigurationAsync(config);
            _createdConfigIds.Add(id);

            // Act
            var result = await _repository.GetAuditConfigurationAsync(uniqueEndpoint, "GET");

            // Assert
            result.Should().NotBeNull();
            result!.Endpoint.Should().Be(uniqueEndpoint);
            result.HttpMethod.Should().Be("GET");
            result.EntityName.Should().Be("GetConfigTest");
        }

        [Fact]
        public async Task GetAuditConfigurationAsync_WithNonExistingEndpoint_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetAuditConfigurationAsync("/api/nonexistent", "POST");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateConfigurationAsync_WithValidConfig_ShouldReturnTrue()
        {
            // Arrange - Create a config first
            var config = new AuditConfiguration
            {
                Endpoint = $"/api/update/{Guid.NewGuid():N}",
                HttpMethod = "PUT",
                EntityName = "UpdateTest",
                IsEnabled = true,
                TrackPropertyChanges = true,
                CreatedBy = "TestUser"
            };
            var id = await _repository.CreateConfigurationAsync(config);
            _createdConfigIds.Add(id);

            // Modify the config
            config.Id = id;
            config.IsEnabled = false;
            config.Description = "Updated description";

            // Act
            var result = await _repository.UpdateConfigurationAsync(config);

            // Assert
            result.Should().BeTrue();

            // Verify the update
            var updated = await _repository.GetAuditConfigurationAsync(config.Endpoint, config.HttpMethod);
            updated!.IsEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateConfigurationAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var config = new AuditConfiguration
            {
                Id = 999999999,
                IsEnabled = false
            };

            // Act
            var result = await _repository.UpdateConfigurationAsync(config);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllConfigurationsAsync_ShouldReturnAllConfigs()
        {
            // Arrange - Create a test config
            var config = new AuditConfiguration
            {
                Endpoint = $"/api/all/{Guid.NewGuid():N}",
                HttpMethod = "GET",
                EntityName = "GetAllTest",
                IsEnabled = true,
                CreatedBy = "TestUser"
            };
            var id = await _repository.CreateConfigurationAsync(config);
            _createdConfigIds.Add(id);

            // Act
            var allConfigs = await _repository.GetAllConfigurationsAsync();

            // Assert
            allConfigs.Should().NotBeNull();
            allConfigs.Should().NotBeEmpty();
            allConfigs.Should().Contain(c => c.Id == id);
        }

        [Fact]
        public async Task GetPagedAsync_WithSearchTerm_ShouldReturnMatchingResults()
        {
            // Arrange
            var uniqueSearchTerm = $"SearchTest_{Guid.NewGuid():N}";
            var entry = new AuditEntry
            {
                EntityName = "SearchEntity",
                EntityId = "1",
                Action = "CREATE",
                UserId = "SearchUser",
                Description = uniqueSearchTerm,
                Timestamp = DateTime.UtcNow
            };
            var id = await _repository.CreateAsync(entry);
            _createdAuditIds.Add(id);

            var parameters = new AuditQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = uniqueSearchTerm
            };

            // Act
            var result = await _repository.GetPagedAsync(parameters);

            // Assert
            result.Data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetPagedAsync_WithDateRange_ShouldReturnFilteredResults()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var entry = new AuditEntry
            {
                EntityName = "DateRangeTest",
                EntityId = "1",
                Action = "CREATE",
                UserId = "DateUser",
                Timestamp = now
            };
            var id = await _repository.CreateAsync(entry);
            _createdAuditIds.Add(id);

            var parameters = new AuditQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                FromDate = now.AddMinutes(-5),
                ToDate = now.AddMinutes(5),
                EntityName = "DateRangeTest"
            };

            // Act
            var result = await _repository.GetPagedAsync(parameters);

            // Assert
            result.Data.Should().NotBeEmpty();
            result.Data.Should().Contain(x => x.EntityName == "DateRangeTest");
        }

        // Cleanup is handled by IntegrationTestBase via Respawner
    }
}

