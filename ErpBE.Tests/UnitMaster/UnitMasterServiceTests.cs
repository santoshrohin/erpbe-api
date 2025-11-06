using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ErpBE.Application.UnitMaster;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.Common.Models;
using ErpBE.Application.Audit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ErpBE.Tests.UnitMaster
{
    public class UnitMasterServiceTests
    {
        private readonly IUnitMasterRepository _mockRepository;
        private readonly ILogger<UnitMasterService> _mockLogger;
        private readonly IAuditService _mockAuditService;
        private readonly UnitMasterService _service;

        public UnitMasterServiceTests()
        {
            _mockRepository = Substitute.For<IUnitMasterRepository>();
            _mockLogger = Substitute.For<ILogger<UnitMasterService>>();
            _mockAuditService = Substitute.For<IAuditService>();
            _service = new UnitMasterService(_mockRepository, _mockLogger, _mockAuditService);
        }

        [Fact]
        public async Task CreateUnitMasterAsync_WithValidRequest_ShouldReturnUnitId()
        {
            // Arrange
            var request = new CreateUnitMasterRequest
            {
                UnitName = "TEST_UNIT",
                UnitDescription = "Test Unit Description",
                CompanyId = 1,
                IsActive = true
            };

            var expectedUnitId = 123;
            // Service no longer validates - that's done in FluentValidation
            _mockRepository.CreateUnitMasterAsync(request)
                          .Returns(expectedUnitId);
            // Setup audit service (can't use Verify due to optional parameters in interface)
            _mockAuditService.LogCreateAsync(
                Arg.Any<string>(), 
                Arg.Any<int>(), 
                Arg.Any<object>(), 
                Arg.Any<string>(),
                null, null, null)
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateUnitMasterAsync(request);

            // Assert
            result.Should().Be(expectedUnitId);
            await _mockRepository.Received(1).CreateUnitMasterAsync(request);
            // Verify audit service was called
            await _mockAuditService.Received().LogCreateAsync(
                Arg.Any<string>(), 
                Arg.Any<int>(), 
                Arg.Any<object>(), 
                Arg.Any<string>(),
                null, null, null);
        }

        // NOTE: Validation is now handled by FluentValidation in the MediatR pipeline
        // This test has been removed as the service no longer performs validation

        [Fact]
        public async Task GetUnitMasterByIdAsync_WithValidId_ShouldReturnUnit()
        {
            // Arrange
            var unitId = 123;
            var expectedUnit = new UnitMasterDto
            {
                Id = unitId,
                CompanyId = 1,
                UnitName = "TEST_UNIT",
                UnitDescription = "Test Unit Description",
                IsActive = true
            };

            _mockRepository.GetUnitMasterByIdAsync(unitId)
                          .Returns(expectedUnit);

            // Act
            var result = await _service.GetUnitMasterByIdAsync(unitId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(unitId);
            result!.UnitName.Should().Be("TEST_UNIT");
            await _mockRepository.Received(1).GetUnitMasterByIdAsync(unitId);
        }

        [Fact]
        public async Task GetUnitMastersAsync_WithValidParameters_ShouldReturnPagedResponse()
        {
            // Arrange
            var queryParameters = new UnitMasterQueryParameters
            {
                CompanyId = 1,
                IsActive = true,
                PageNumber = 1,
                PageSize = 10
            };

            var expectedResponse = new PagedResponse<UnitMasterDto>
            {
                Data = new List<UnitMasterDto>
                {
                    new UnitMasterDto { Id = 1, UnitName = "UNIT1" },
                    new UnitMasterDto { Id = 2, UnitName = "UNIT2" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.GetUnitMastersAsync(queryParameters)
                          .Returns(expectedResponse);

            // Act
            var result = await _service.GetUnitMastersAsync(queryParameters);

            // Assert
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2);
            result!.Data.Should().HaveCount(2);
            await _mockRepository.Received(1).GetUnitMastersAsync(queryParameters);
        }

        [Fact]
        public async Task SetUnitMasterActiveStatusAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var unitId = 123;
            var isActive = false;

            _mockRepository.SetUnitMasterActiveStatusAsync(unitId, isActive)
                          .Returns(true);

            // Act
            var result = await _service.SetUnitMasterActiveStatusAsync(unitId, isActive);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).SetUnitMasterActiveStatusAsync(unitId, isActive);
        }
    }
}
