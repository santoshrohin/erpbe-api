using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ErpBE.API.Common;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;
using ErpBE.Application.ItemCategoryMaster.Commands;
using ErpBE.Application.ItemCategoryMaster.Queries;
using ErpBE.Domain.Common;

namespace ErpBE.API.Controllers.Master
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItemCategoryMasterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ItemCategoryMasterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new item category.
        /// </summary>
        [HttpPost]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Add)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateItemCategory([FromBody] CreateItemCategoryMasterRequest request)
        {
            var command = new CreateItemCategoryMasterCommand { Request = request };
            var categoryId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetItemCategoryById), new { id = categoryId }, categoryId);
        }

        /// <summary>
        /// Updates an existing item category.
        /// </summary>
        [HttpPut]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Edit)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemCategory([FromBody] UpdateItemCategoryMasterRequest request)
        {
            var command = new UpdateItemCategoryMasterCommand { Request = request };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Deletes an item category by ID (soft delete).
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteItemCategory(int id)
        {
            var command = new DeleteItemCategoryMasterCommand { CategoryId = id };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Gets an item category by ID.
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(ItemCategoryMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItemCategoryById(int id)
        {
            var query = new GetItemCategoryMasterByIdQuery { CategoryId = id };
            var category = await _mediator.Send(query);
            return Ok(category);
        }

        /// <summary>
        /// Gets a paginated list of item categories with server-side filtering, searching, and sorting.
        /// </summary>
        [HttpGet]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(PagedResponse<ItemCategoryMasterDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetItemCategories([FromQuery] ItemCategoryMasterQueryParameters queryParameters)
        {
            var query = new GetItemCategoryMastersQuery { QueryParameters = queryParameters };
            var categories = await _mediator.Send(query);
            return Ok(categories);
        }

        /// <summary>
        /// Gets an item category by name and company ID.
        /// </summary>
        [HttpGet("name/{categoryName}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(ItemCategoryMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItemCategoryByName(string categoryName, [FromQuery] int companyId)
        {
            var query = new GetItemCategoryMasterByNameQuery 
            { 
                CategoryName = categoryName.ToUpper(), 
                CompanyId = companyId 
            };
            var category = await _mediator.Send(query);
            return Ok(category);
        }

        /// <summary>
        /// Checks if an item category name is unique within a company.
        /// </summary>
        [HttpGet("check-unique")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckCategoryNameUnique(
            [FromQuery] string categoryName, 
            [FromQuery] int companyId, 
            [FromQuery] int? excludeId = null)
        {
            var query = new CheckItemCategoryNameUniqueQuery 
            { 
                CategoryName = categoryName.ToUpper(), 
                CompanyId = companyId, 
                ExcludeCategoryId = excludeId 
            };
            var isUnique = await _mediator.Send(query);
            return Ok(new { isUnique });
        }
    }
}



