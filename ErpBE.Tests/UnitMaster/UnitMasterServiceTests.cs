using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ErpBE.Application.UnitMaster;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.CommonDto;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ErpBE.Tests.UnitMaster
{
    public class UnitMasterServiceTests
    {
        private readonly Mock<IUnitMasterRepository> _mockRepository;
        private readonly Mock<ILogger<UnitMasterService>> _mockLogger;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly UnitMasterService _service;

        public UnitMasterServiceTests()
        {
            _mockRepository = new Mock<IUnitMasterRepository>();
            _mockLogger = new Mock<ILogger<UnitMasterService>>();
            _mockAuditService = new Mock<IAuditService>();
            _service = new UnitMasterService(_mockRepository.Object, _mockLogger.Object, _mockAuditService.Object);
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
            _mockRepository.Setup(x => x.IsUnitNameUniqueAsync(request.UnitName, request.CompanyId, null))
                          .ReturnsAsync(true);
            _mockRepository.Setup(x => x.CreateUnitMasterAsync(request))
                          .ReturnsAsync(expectedUnitId);

            // Act
            var result = await _service.CreateUnitMasterAsync(request);

            // Assert
            result.Should().Be(expectedUnitId);
            _mockRepository.Verify(x => x.IsUnitNameUniqueAsync(request.UnitName, request.CompanyId, null), Times.Once);
            _mockRepository.Verify(x => x.CreateUnitMasterAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreateUnitMasterAsync_WithDuplicateName_ShouldThrowException()
        {
            // Arrange
            var request = new CreateUnitMasterRequest
            {
                UnitName = "EXISTING_UNIT",
                UnitDescription = "Test Unit Description",
                CompanyId = 1,
                IsActive = true
            };

            _mockRepository.Setup(x => x.IsUnitNameUniqueAsync(request.UnitName, request.CompanyId, null))
                          .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUnitMasterAsync(request));
            _mockRepository.Verify(x => x.CreateUnitMasterAsync(request), Times.Never);
        }

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

            _mockRepository.Setup(x => x.GetUnitMasterByIdAsync(unitId))
                          .ReturnsAsync(expectedUnit);

            // Act
            var result = await _service.GetUnitMasterByIdAsync(unitId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(unitId);
            result!.UnitName.Should().Be("TEST_UNIT");
            _mockRepository.Verify(x => x.GetUnitMasterByIdAsync(unitId), Times.Once);
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

            _mockRepository.Setup(x => x.GetUnitMastersAsync(queryParameters))
                          .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.GetUnitMastersAsync(queryParameters);

            // Assert
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2);
            result!.Data.Should().HaveCount(2);
            _mockRepository.Verify(x => x.GetUnitMastersAsync(queryParameters), Times.Once);
        }

        [Fact]
        public async Task SetUnitMasterActiveStatusAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var unitId = 123;
            var isActive = false;

            _mockRepository.Setup(x => x.SetUnitMasterActiveStatusAsync(unitId, isActive))
                          .ReturnsAsync(true);

            // Act
            var result = await _service.SetUnitMasterActiveStatusAsync(unitId, isActive);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(x => x.SetUnitMasterActiveStatusAsync(unitId, isActive), Times.Once);
        }
    }
}
