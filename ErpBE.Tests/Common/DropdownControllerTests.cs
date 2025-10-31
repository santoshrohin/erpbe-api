using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ErpBE.Application.Common.Models;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ErpBE.Tests.Common
{
    public class DropdownControllerTests : TestBase
    {
        public DropdownControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task BatchDropdown_MultiMaster_WorksAndReturnsPaging()
        {
            // Arrange
            var request = new BatchDropdownRequest
            {
                Requests = new List<NamedDropdownBatchRequest>
                {
                    new NamedDropdownBatchRequest
                    {
                        Key = "customer",
                        Request = new DropdownRequest
                        {
                            Table = "PARTY_MASTER",
                            IdColumn = "P_CODE",
                            DisplayColumn = "P_NAME",
                            Where = "ES_DELETE = 0 AND P_ACTIVE_IND = 1",
                            OrderBy = "P_NAME ASC",
                            SearchText = string.Empty
                        }
                    },
                    new NamedDropdownBatchRequest
                    {
                        Key = "poType",
                        Request = new DropdownRequest
                        {
                            Table = "PO_TYPE_MASTER",
                            IdColumn = "PO_T_CODE",
                            DisplayColumn = "PO_T_DESC",
                            Where = "ES_DELETE = 0",
                            OrderBy = "PO_T_DESC ASC",
                            SearchText = string.Empty
                        }
                    },
                    new NamedDropdownBatchRequest
                    {
                        Key = "item",
                        Request = new DropdownRequest
                        {
                            Table = "ITEM_MASTER",
                            IdColumn = "I_CODE",
                            DisplayColumn = "I_NAME",
                            Where = "ES_DELETE = 0 AND I_ACTIVE_IND = 1",
                            OrderBy = "I_NAME ASC",
                            Skip = 0,
                            Take = 10,
                            SearchText = string.Empty
                        }
                    }
                }
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/Dropdown/batch", request);
            var responseBody = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK, "Response: {0}", responseBody);
            var data = await response.Content.ReadFromJsonAsync<BatchDropdownResponse>();

            // Assert
            data.Should().NotBeNull();
            data!.Data.Should().ContainKey("customer");
            data.Data.Should().ContainKey("poType");
            data.Data.Should().ContainKey("item");
            data.Data["item"].Count.Should().BeLessOrEqualTo(10);
            var firstItem = data.Data["item"].FirstOrDefault();
            if (firstItem != null && firstItem.TotalCount.HasValue)
                firstItem.TotalCount.Should().BeGreaterThan(0);
        }
    }
}
