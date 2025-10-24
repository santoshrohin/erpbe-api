using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Sales
{
    public class TaxInvoiceControllerTests : IntegrationTestBase
    {
        public TaxInvoiceControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region Create Tests

        [Fact]
        public async Task CreateTaxInvoice_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1, // Assuming customer exists
                CustomerPoCode = 1, // Assuming PO exists
                InvoiceType = 0,
                Type = "TAXINV",
                Remarks = "Test Tax Invoice",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1, // Assuming item exists
                        UomCode = 1,
                        InvoiceQuantity = 10,
                        Rate = 100.50
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();
            result.Should().NotBeNull();
            result!.InvoiceCode.Should().NotBe(0);
            result.CompanyCode.Should().Be(1);
            result.NetAmount.Should().BeGreaterThan(0);
            result.InvoiceDetails.Should().HaveCount(1);

            // Cleanup
            await Client.DeleteAsync($"/api/TaxInvoice/{result.InvoiceCode}?companyId=1");
        }

        [Fact]
        public async Task CreateTaxInvoice_WithoutLineItems_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>() // Empty
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateTaxInvoice_WithoutCustomerPo_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = null, // Missing (MANDATORY)
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateTaxInvoice_WithGstCalculations_ShouldCalculateCorrectly()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1,
                        UomCode = 1,
                        InvoiceQuantity = 100,
                        Rate = 10, // 100 * 10 = 1000
                        CgstPercentage = 9, // CGST = 90
                        SgstPercentage = 9  // SGST = 90
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();
            result.Should().NotBeNull();
            result!.NetAmount.Should().Be(1000);
            result.BasicExciseAmount.Should().Be(90); // CGST
            result.EducationCessAmount.Should().Be(90); // SGST
            result.GrossAmount.Should().Be(1180); // 1000 + 90 + 90 = 1180

            // Cleanup
            await Client.DeleteAsync($"/api/TaxInvoice/{result.InvoiceCode}?companyId=1");
        }

        [Fact]
        public async Task CreateTaxInvoice_WithDiscount_ShouldCalculateCorrectly()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                DiscountPercentage = 10, // 10% discount
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1,
                        UomCode = 1,
                        InvoiceQuantity = 100,
                        Rate = 10 // Net = 1000
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();
            result.Should().NotBeNull();
            result!.NetAmount.Should().Be(1000);
            result.DiscountAmount.Should().Be(100); // 10% of 1000
            result.AccessibleAmount.Should().Be(900); // 1000 - 100

            // Cleanup
            await Client.DeleteAsync($"/api/TaxInvoice/{result.InvoiceCode}?companyId=1");
        }

        #endregion

        #region Get Tests

        [Fact]
        public async Task GetTaxInvoiceById_WithValidId_ShouldReturnInvoice()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create invoice first
            var createRequest = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            var createResponse = await Client.PostAsJsonAsync("/api/TaxInvoice", createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();

            // Act
            var response = await Client.GetAsync($"/api/TaxInvoice/{created!.InvoiceCode}?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();
            result.Should().NotBeNull();
            result!.InvoiceCode.Should().Be(created.InvoiceCode);
            result.InvoiceDetails.Should().NotBeEmpty();

            // Cleanup
            await Client.DeleteAsync($"/api/TaxInvoice/{created.InvoiceCode}?companyId=1");
        }

        [Fact]
        public async Task GetTaxInvoiceById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/TaxInvoice/999999999?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllTaxInvoices_ShouldReturnPagedList()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/TaxInvoice?CompanyId=1&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoicePagedResponse>();
            result.Should().NotBeNull();
            result!.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetAllTaxInvoices_WithFilters_ShouldFilterCorrectly()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create test invoice
            var createRequest = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };
            var createResponse = await Client.PostAsJsonAsync("/api/TaxInvoice", createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();

            // Act
            var response = await Client.GetAsync($"/api/TaxInvoice?CompanyId=1&CustomerId=1&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoicePagedResponse>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
            result.Data.Should().Contain(i => i.InvoiceCode == created!.InvoiceCode);

            // Cleanup
            await Client.DeleteAsync($"/api/TaxInvoice/{created!.InvoiceCode}?companyId=1");
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateTaxInvoice_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create invoice first
            var createRequest = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                Remarks = "Original Remarks",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            var createResponse = await Client.PostAsJsonAsync("/api/TaxInvoice", createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();

            // Update request
            var updateRequest = new UpdateTaxInvoiceCommand
            {
                InvoiceCode = created!.InvoiceCode,
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                Remarks = "Updated Remarks",
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 20, Rate = 150 }
                }
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/TaxInvoice/{created.InvoiceCode}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();
            result.Should().NotBeNull();
            result!.Remarks.Should().Be("Updated Remarks");
            result.InvoiceDetails.First().InvoiceQuantity.Should().Be(20);
            result.InvoiceDetails.First().Rate.Should().Be(150);

            // Cleanup
            await Client.DeleteAsync($"/api/TaxInvoice/{created.InvoiceCode}?companyId=1");
        }

        [Fact]
        public async Task UpdateTaxInvoice_WithMismatchedId_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new UpdateTaxInvoiceCommand
            {
                InvoiceCode = 100,
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/TaxInvoice/200", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteTaxInvoice_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create invoice first
            var createRequest = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            var createResponse = await Client.PostAsJsonAsync("/api/TaxInvoice", createRequest);
            var created = await createResponse.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();

            // Act
            var response = await Client.DeleteAsync($"/api/TaxInvoice/{created!.InvoiceCode}?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify soft deletion - invoice still exists but IsDeleted flag is set
            var getResponse = await Client.GetAsync($"/api/TaxInvoice/{created.InvoiceCode}?companyId=1");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var deletedInvoice = await getResponse.Content.ReadFromJsonAsync<TaxInvoiceMasterDto>();
            deletedInvoice.Should().NotBeNull();
            deletedInvoice!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTaxInvoice_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.DeleteAsync("/api/TaxInvoice/999999999?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task CreateTaxInvoice_WithInvalidDiscountPercentage_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                DiscountPercentage = 150, // Invalid: > 100
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand { ItemCode = 1, UomCode = 1, InvoiceQuantity = 10, Rate = 100 }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateTaxInvoice_WithBothCgstSgstAndIgst_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1,
                        UomCode = 1,
                        InvoiceQuantity = 10,
                        Rate = 100,
                        CgstPercentage = 9, // Intra-state
                        SgstPercentage = 9,
                        IgstPercentage = 18 // Inter-state - INVALID!
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateTaxInvoice_WithZeroQuantity_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1,
                        UomCode = 1,
                        InvoiceQuantity = 0, // Invalid
                        Rate = 100
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateTaxInvoice_WithZeroRate_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateTaxInvoiceCommand
            {
                CompanyCode = 1,
                InvoiceDate = DateTime.Now,
                CustomerCode = 1,
                CustomerPoCode = 1,
                InvoiceDetails = new List<CreateTaxInvoiceDetailCommand>
                {
                    new CreateTaxInvoiceDetailCommand
                    {
                        ItemCode = 1,
                        UomCode = 1,
                        InvoiceQuantity = 10,
                        Rate = 0 // Invalid
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/TaxInvoice", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion
    }
}

