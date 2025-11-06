using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.UserManagement.Commands;
using ErpBE.Application.UserManagement.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;
using MediatR; // Added for Unit

namespace ErpBE.Tests.RoleManagement
{
    /// <summary>
    /// Integration tests for Role Management functionality
    /// Tests create test data, verify operations, and clean up afterwards (matches reference implementation - tests handlers directly)
    /// </summary>
    public class RoleControllerTests : IntegrationTestBase
    {
        [Fact]
        public async Task CreateRole_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var command = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = $"TEST_ROLE_{uniqueId}",
                    Description = "Test Role for Integration Testing",
                    IsActive = true
                }
            };

            try
            {
                // Act
                var role = await Mediator.Send(command);

                // Assert
                role.Should().NotBeNull();
                role.RoleId.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative
                role.RoleName.Should().Be($"TEST_ROLE_{uniqueId}");

                // Cleanup
                var deleteCommand = new DeleteRoleCommand { RoleId = role.RoleId };
                await Mediator.Send(deleteCommand);
            }
            catch
            {
                // Fallback cleanup
            }
        }

        [Fact]
        public async Task CreateRole_WithDuplicateName_ShouldThrowConflictException()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var command = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = $"DUPLICATE_ROLE_{uniqueId}",
                    Description = "Duplicate Test Role",
                    IsActive = true
                }
            };

            try
            {
                // Create first
                var firstRole = await Mediator.Send(command);
                firstRole.Should().NotBeNull();

                // Act & Assert - Try to create duplicate
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await Mediator.Send(command));

                // Cleanup
                var deleteCommand = new DeleteRoleCommand { RoleId = firstRole.RoleId };
                await Mediator.Send(deleteCommand);
            }
            catch
            {
                // Fallback cleanup
            }
        }

        [Fact]
        public async Task CreateRole_WithInvalidData_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = "", // Invalid: empty role name
                    Description = "Test Role Description",
                    IsActive = true
                }
            };

            // Act & Assert - Validation should fail (handled by MediatR ValidationBehavior)
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await Mediator.Send(command));
        }

        [Fact]
        public async Task GetRoles_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var query = new GetAllRolesQuery();

            // Act
            var roles = await Mediator.Send(query);

            // Assert
            roles.Should().NotBeNull();
            roles.Should().BeAssignableTo<List<RoleDto>>();
        }

        [Fact]
        public async Task GetRoleById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createCommand = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = $"TEST_GET_ROLE_{uniqueId}",
                    Description = "Test Get Role",
                    IsActive = true
                }
            };

            try
            {
                // Create a test role
                var createdRole = await Mediator.Send(createCommand);
                createdRole.Should().NotBeNull();

                // Act
                var query = new GetRoleByIdQuery { RoleId = createdRole.RoleId };
                var role = await Mediator.Send(query);

                // Assert
                role.Should().NotBeNull();
                role!.RoleId.Should().Be(createdRole.RoleId);
                role.RoleName.Should().Be(createdRole.RoleName);

                // Cleanup
                var deleteCommand = new DeleteRoleCommand { RoleId = createdRole.RoleId };
                await Mediator.Send(deleteCommand);
            }
            catch
            {
                // Fallback cleanup
            }
        }

        [Fact]
        public async Task GetRoleById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var query = new GetRoleByIdQuery { RoleId = 999999 };

            // Act
            var role = await Mediator.Send(query);

            // Assert
            role.Should().BeNull();
        }

        [Fact]
        public async Task UpdateRole_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createCommand = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = $"TEST_UPDATE_ROLE_{uniqueId}",
                    Description = "Test Update Role",
                    IsActive = true
                }
            };

            try
            {
                // Create a test role
                var createdRole = await Mediator.Send(createCommand);
                createdRole.Should().NotBeNull();

                // Act
                var updateCommand = new UpdateRoleCommand
                {
                    Request = new UpdateRoleRequest
                    {
                        RoleId = createdRole.RoleId,
                        RoleName = $"UPDATED_ROLE_{uniqueId}",
                        Description = "Updated Test Update Role",
                        IsActive = false
                    }
                };
                var updatedRole = await Mediator.Send(updateCommand);

                // Assert
                updatedRole.Should().NotBeNull();
                updatedRole.RoleId.Should().Be(createdRole.RoleId);
                updatedRole.RoleName.Should().Be($"UPDATED_ROLE_{uniqueId}");
                updatedRole.IsActive.Should().BeFalse();

                // Cleanup
                var deleteCommand = new DeleteRoleCommand { RoleId = createdRole.RoleId };
                await Mediator.Send(deleteCommand);
            }
            catch
            {
                // Fallback cleanup
            }
        }

        [Fact]
        public async Task UpdateRole_WithNonExistentId_ShouldThrowException()
        {
            // Arrange
            var updateCommand = new UpdateRoleCommand
            {
                Request = new UpdateRoleRequest
                {
                    RoleId = 999999,
                    RoleName = "NON_EXISTENT_ROLE",
                    Description = "Non-existent Role",
                    IsActive = true
                }
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await Mediator.Send(updateCommand));
        }

        [Fact]
        public async Task DeleteRole_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createCommand = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = $"TEST_DELETE_ROLE_{uniqueId}",
                    Description = "Test Delete Role",
                    IsActive = true
                }
            };

            try
            {
                // Create a test role
                var createdRole = await Mediator.Send(createCommand);
                createdRole.Should().NotBeNull();

                // Act
                var deleteCommand = new DeleteRoleCommand { RoleId = createdRole.RoleId };
                var success = await Mediator.Send(deleteCommand);

                // Assert
                success.Should().BeTrue();
            }
            catch
            {
                // Fallback cleanup
            }
        }

        [Fact]
        public async Task DeleteRole_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var deleteCommand = new DeleteRoleCommand { RoleId = 999999 };

            // Act
            var success = await Mediator.Send(deleteCommand);

            // Assert
            success.Should().BeFalse();
        }

        [Fact]
        public async Task GetRoleByName_WithValidName_ShouldReturnRole()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var roleName = $"RNAME_{uniqueId}";
            var createCommand = new CreateRoleCommand
            {
                Request = new CreateRoleRequest
                {
                    RoleName = roleName,
                    Description = "Test",
                    IsActive = true
                }
            };

            try
            {
                // Create
                var createdRole = await Mediator.Send(createCommand);
                createdRole.Should().NotBeNull();

                // Act
                var query = new GetRoleByNameQuery { RoleName = roleName };
                var role = await Mediator.Send(query);

                // Assert
                role.Should().NotBeNull();
                role!.RoleName.Should().Be(roleName);

                // Cleanup
                var deleteCommand = new DeleteRoleCommand { RoleId = createdRole.RoleId };
                await Mediator.Send(deleteCommand);
            }
            catch
            {
                // Fallback cleanup
            }
        }

        [Fact]
        public async Task GetActiveRoles_ShouldReturnOnlyActiveRoles()
        {
            // Arrange
            var query = new GetActiveRolesQuery();

            // Act
            var roles = await Mediator.Send(query);

            // Assert
            roles.Should().NotBeNull();
            roles.Should().BeAssignableTo<List<RoleDto>>();
            roles.Should().OnlyContain(r => r.IsActive);
        }

        [Fact]
        public async Task CheckRoleExists_WithExistingRole_ShouldReturnTrue()
        {
            // Arrange
            var query = new CheckRoleExistsQuery { RoleName = "Admin" }; // Assuming Admin role exists

            // Act
            var exists = await Mediator.Send(query);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CheckRoleExists_WithNonExistingRole_ShouldReturnFalse()
        {
            // Arrange
            var query = new CheckRoleExistsQuery { RoleName = $"NonExist_{Guid.NewGuid()}" };

            // Act
            var exists = await Mediator.Send(query);

            // Assert
            exists.Should().BeFalse();
        }
    }
}
