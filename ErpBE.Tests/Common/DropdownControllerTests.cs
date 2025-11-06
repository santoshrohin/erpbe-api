using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Common;
using ErpBE.Application.Common.Models;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Common;

/// <summary>
/// Integration tests for Dropdown functionality
/// Tests dropdown data retrieval (matches reference implementation - tests handlers directly)
/// </summary>
public class DropdownControllerTests : IntegrationTestBase
{
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

        var query = new GetBatchDropdownsQuery(request);

        // Act
        var data = await Mediator.Send(query);

        // Assert
        data.Should().NotBeNull();
        data!.Data.Should().ContainKey("customer");
        data.Data.Should().ContainKey("poType");
        data.Data.Should().ContainKey("item");
        data.Data["item"].Count.Should().BeLessOrEqualTo(10);
        var firstItem = data.Data["item"].FirstOrDefault();
        if (firstItem != null && firstItem.TotalCount.HasValue)
        {
            firstItem.TotalCount.Should().BeGreaterThan(0);
        }
    }
}
