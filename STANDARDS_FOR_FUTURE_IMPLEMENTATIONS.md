# 📋 Standards for Future Implementations

## ✅ **Mandatory Principles to Follow**

Based on UnitMaster and ItemCategoryMaster implementations, these standards MUST be followed for all future modules:

---

## 1. **Validations → FluentValidation (ONLY)**

### ✅ **DO:**
- Create validators in `ErpBE.Application/[Module]/Validators/`
- Validate **request parameters** (required, length, format, range)
- Use `AbstractValidator<T>` for all DTOs
- Register validators automatically via `ValidationBehavior`

### ❌ **DON'T:**
- Validate in controllers
- Validate in command/query handlers (except business rules)
- Use DataAnnotations attributes

### **Example:**
```csharp
public class CreateItemCategoryMasterRequestValidator : AbstractValidator<CreateItemCategoryMasterRequest>
{
    public CreateItemCategoryMasterRequestValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category Name is required.")
            .MaximumLength(50).WithMessage("Category Name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Category Name contains invalid characters.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Company ID is required.");
    }
}
```

---

## 2. **Controllers → Thin (NO Business Logic)**

### ✅ **DO:**
- Accept requests
- Send commands/queries via `IMediator`
- Return results
- Handle HTTP status codes

### ❌ **DON'T:**
- Check for null
- Validate data
- Apply business rules
- Access repositories directly
- Have conditional logic

### **Example:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetItemCategoryById(int id)
{
    var query = new GetItemCategoryMasterByIdQuery { CategoryId = id };
    var category = await _mediator.Send(query);
    return Ok(category); // No null check here!
}
```

---

## 3. **Null Checks → Query/Command Handlers**

### ✅ **DO:**
- Check for null in handlers
- Throw `KeyNotFoundException` for 404 scenarios
- Throw `InvalidOperationException` for 400 scenarios
- Let global exception filter handle HTTP responses

### ❌ **DON'T:**
- Return null from handlers
- Check null in controllers
- Return `NotFound()` or `BadRequest()` from handlers

### **Example:**
```csharp
public async Task<ItemCategoryMasterDto> Handle(GetItemCategoryMasterByIdQuery request, CancellationToken cancellationToken)
{
    var category = await _repository.GetByIdAsync(request.CategoryId);
    
    if (category == null)
    {
        throw new KeyNotFoundException($"Item Category with ID '{request.CategoryId}' not found.");
    }
    
    return category;
}
```

---

## 4. **Exception Handling → Global Exception Filter**

### ✅ **DO:**
- Use `ValidationExceptionHandler` (already configured)
- Throw exceptions from handlers
- Let filter convert to HTTP responses:
  - `ValidationException` → 400 BadRequest
  - `InvalidOperationException` → 400 BadRequest
  - `KeyNotFoundException` → 404 NotFound

### ❌ **DON'T:**
- Catch exceptions in controllers
- Return error objects manually
- Use try-catch in handlers

---

## 5. **Stored Procedures → SET NOCOUNT OFF**

### ✅ **DO:**
```sql
CREATE OR ALTER PROCEDURE [dbo].[SP_CreateItemCategoryMaster]
    @I_CAT_NAME NVARCHAR(50),
    @I_CAT_CM_COMP_ID INT
AS
BEGIN
    SET NOCOUNT OFF; -- ✅ REQUIRED for @@ROWCOUNT
    
    INSERT INTO ITEM_CATEGORY_MASTER (I_CAT_NAME, I_CAT_CM_COMP_ID)
    VALUES (@I_CAT_NAME, @I_CAT_CM_COMP_ID);
    
    SELECT SCOPE_IDENTITY() AS CategoryId;
END
```

### ❌ **DON'T:**
```sql
SET NOCOUNT ON; -- ❌ WRONG - breaks ExecuteAsync
```

---

## 6. **Server-Side Operations (ALWAYS)**

### ✅ **DO:**
- Pagination in stored procedure
- Filtering in stored procedure
- Sorting in stored procedure
- Search in stored procedure

### ❌ **DON'T:**
- Load all data and filter in memory
- Use `.ToList()` before filtering
- Use LINQ for filtering database results

### **Example:**
```sql
CREATE OR ALTER PROCEDURE [dbo].[SP_GetItemCategoryMasters]
    @PageNumber INT = 1,
    @PageSize INT = 15,
    @SortBy NVARCHAR(50) = 'I_CAT_NAME',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(200) = NULL,
    @CompanyId INT = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM ITEM_CATEGORY_MASTER
    WHERE (@CompanyId IS NULL OR I_CAT_CM_COMP_ID = @CompanyId)
      AND (@SearchTerm IS NULL OR I_CAT_NAME LIKE '%' + @SearchTerm + '%');
    
    -- Get paginated results
    SELECT * FROM ITEM_CATEGORY_MASTER
    WHERE (@CompanyId IS NULL OR I_CAT_CM_COMP_ID = @CompanyId)
      AND (@SearchTerm IS NULL OR I_CAT_NAME LIKE '%' + @SearchTerm + '%')
    ORDER BY 
        CASE WHEN @SortBy = 'I_CAT_NAME' AND @SortDirection = 'ASC' THEN I_CAT_NAME END ASC,
        CASE WHEN @SortBy = 'I_CAT_NAME' AND @SortDirection = 'DESC' THEN I_CAT_NAME END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
```

---

## 7. **Clean Architecture Layers**

### **Structure:**
```
ErpBE.Domain/           → Entities, Interfaces (no dependencies)
ErpBE.Application/      → DTOs, Commands, Queries, Handlers, Validators (depends on Domain)
ErpBE.Infrastructure/   → Repositories, DB Context (depends on Application & Domain)
ErpBE.API/              → Controllers, Middleware (depends on Application)
ErpBE.Tests/            → Unit & Integration Tests
```

### **Dependency Rules:**
- Domain → No dependencies
- Application → Domain only
- Infrastructure → Application, Domain
- API → Application only (NOT Infrastructure)

---

## 8. **CQRS Pattern with MediatR**

### **Commands (Write Operations):**
```csharp
// Command
public class CreateItemCategoryMasterCommand : IRequest<int>
{
    public CreateItemCategoryMasterRequest Request { get; set; }
}

// Handler
public class CreateItemCategoryMasterCommandHandler : IRequestHandler<CreateItemCategoryMasterCommand, int>
{
    private readonly IItemCategoryMasterRepository _repository;

    public async Task<int> Handle(CreateItemCategoryMasterCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
        request.Request.CategoryName = request.Request.CategoryName.ToUpper().Trim();
        
        // Validate business rules
        var isUnique = await _repository.IsCategoryNameUniqueAsync(...);
        if (!isUnique)
        {
            throw new InvalidOperationException("Item Category already exists.");
        }
        
        return await _repository.CreateAsync(request.Request);
    }
}
```

### **Queries (Read Operations):**
```csharp
// Query
public class GetItemCategoryMasterByIdQuery : IRequest<ItemCategoryMasterDto>
{
    public int CategoryId { get; set; }
}

// Handler
public class GetItemCategoryMasterByIdQueryHandler : IRequestHandler<GetItemCategoryMasterByIdQuery, ItemCategoryMasterDto>
{
    private readonly IItemCategoryMasterRepository _repository;

    public async Task<ItemCategoryMasterDto> Handle(GetItemCategoryMasterByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.CategoryId);
        
        if (category == null)
        {
            throw new KeyNotFoundException($"Item Category with ID '{request.CategoryId}' not found.");
        }
        
        return category;
    }
}
```

---

## 9. **Repository Pattern with Dapper**

### **Interface:**
```csharp
public interface IItemCategoryMasterRepository
{
    Task<int> CreateAsync(CreateItemCategoryMasterRequest request);
    Task<bool> UpdateAsync(UpdateItemCategoryMasterRequest request);
    Task<bool> DeleteAsync(int categoryId);
    Task<ItemCategoryMasterDto?> GetByIdAsync(int categoryId);
    Task<PagedResponse<ItemCategoryMasterDto>> GetPagedAsync(ItemCategoryMasterQueryParameters queryParameters);
}
```

### **Implementation:**
```csharp
public class ItemCategoryMasterRepository : IItemCategoryMasterRepository
{
    private readonly IDbConnection _db;

    public async Task<int> CreateAsync(CreateItemCategoryMasterRequest request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@I_CAT_NAME", request.CategoryName);
        parameters.Add("@I_CAT_CM_COMP_ID", request.CompanyId);

        var categoryId = await _db.QuerySingleAsync<int>(
            "SP_CreateItemCategoryMaster",
            parameters,
            commandType: CommandType.StoredProcedure);

        return categoryId;
    }
}
```

---

## 10. **Test Coverage (100% Required)**

### **Validator Tests:**
```csharp
[Fact]
public void Validate_WithValidRequest_ShouldPass()
{
    var request = new CreateItemCategoryMasterRequest
    {
        CategoryName = "ELECTRONICS",
        CompanyId = 1
    };
    
    var result = _validator.Validate(request);
    
    result.IsValid.Should().BeTrue();
}

[Theory]
[InlineData("")]
[InlineData(" ")]
[InlineData(null)]
public void Validate_WithEmptyCategoryName_ShouldFail(string categoryName)
{
    var request = new CreateItemCategoryMasterRequest
    {
        CategoryName = categoryName!,
        CompanyId = 1
    };
    
    var result = _validator.Validate(request);
    
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
}
```

### **Controller Integration Tests:**
```csharp
[Fact]
public async Task CreateItemCategory_WithValidData_ShouldReturnCreated()
{
    var token = await GetAuthTokenAsync();
    Client.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var request = new CreateItemCategoryMasterRequest
    {
        CategoryName = "TEST" + Guid.NewGuid().ToString().Substring(0, 4),
        CompanyId = 1
    };

    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
        var response = await Client.PostAsync("/api/ItemCategoryMaster", content);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var categoryId = int.Parse(await response.Content.ReadAsStringAsync());
        categoryId.Should().NotBe(0);
        
        // Cleanup
        await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
    }
    finally
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }
}
```

---

## 11. **Authorization**

### ✅ **DO:**
```csharp
[ApiController]
[Route("api/[controller]")]
[AuthorizeAdmin] // ✅ Admin-only for masters
public class ItemCategoryMasterController : ControllerBase
{
    // ...
}
```

### **Available Attributes:**
- `[AuthorizeAdmin]` - Admin only
- `[AuthorizeSalesManager]` - Sales module
- `[AuthorizeStoreManager]` - Store module
- `[AuthorizePurchaseManager]` - Purchase module
- `[AuthorizeReadOnly]` - Read-only access

---

## 12. **Business Logic Preservation**

### ✅ **DO:**
- Copy exact business rules from legacy application
- Maintain same data transformations (uppercase, trim, etc.)
- Keep same validation rules
- Preserve dependency checks (e.g., check usage before delete)

### **Example from Legacy:**
```csharp
// Legacy: txtItemCategoryName.Text.ToUpper().Trim()
// New: In Command Handler
request.Request.CategoryName = request.Request.CategoryName.ToUpper().Trim();
```

---

## 📊 **File Structure Template**

For each new master, create:

```
ErpBE.Application/
├── DTOs/
│   ├── [Master]Dto.cs
│   ├── Create[Master]Request.cs
│   └── Update[Master]Request.cs
├── Common/Models/
│   └── [Master]QueryParameters.cs
├── [Master]/
│   ├── Commands/
│   │   ├── Create[Master]Command.cs
│   │   ├── Update[Master]Command.cs
│   │   └── Delete[Master]Command.cs
│   ├── Queries/
│   │   ├── Get[Master]ByIdQuery.cs
│   │   ├── Get[Master]ByIdQueryHandler.cs
│   │   ├── Get[Master]sQuery.cs
│   │   ├── Get[Master]sQueryHandler.cs
│   │   ├── Get[Master]ByNameQuery.cs
│   │   ├── Get[Master]ByNameQueryHandler.cs
│   │   ├── Check[Master]NameUniqueQuery.cs
│   │   └── Check[Master]NameUniqueQueryHandler.cs
│   ├── Handlers/
│   │   ├── Create[Master]CommandHandler.cs
│   │   ├── Update[Master]CommandHandler.cs
│   │   └── Delete[Master]CommandHandler.cs
│   └── Validators/
│       ├── Create[Master]RequestValidator.cs
│       ├── Update[Master]RequestValidator.cs
│       └── [Master]QueryParametersValidator.cs
└── Interfaces/
    └── I[Master]Repository.cs

ErpBE.Infrastructure/
└── Repositories/
    └── [Master]Repository.cs

ErpBE.API/
└── Controllers/Master/
    └── [Master]Controller.cs

ErpBE.Tests/
├── [Master]/
│   └── [Master]ControllerTests.cs
└── Validators/
    ├── Create[Master]RequestValidatorTests.cs
    ├── Update[Master]RequestValidatorTests.cs
    └── [Master]QueryParametersValidatorTests.cs

Database_Scripts/
├── StoredProcedures/[Master]/
│   ├── SP_Create[Master].sql
│   ├── SP_Update[Master].sql
│   ├── SP_Delete[Master].sql
│   ├── SP_Get[Master]ById.sql
│   ├── SP_Get[Master]s.sql
│   ├── SP_Get[Master]ByName.sql
│   └── SP_Is[Master]NameUnique.sql
└── Deploy_[Master]_StoredProcedures.sql
```

---

## ✅ **Checklist for Each New Module**

Before marking complete, verify:

- [ ] All validations in validators (NO validation in controllers)
- [ ] Controllers are thin (NO business logic)
- [ ] Null checks in handlers (throw exceptions)
- [ ] All stored procedures use `SET NOCOUNT OFF`
- [ ] Server-side pagination, filtering, sorting
- [ ] Clean Architecture respected
- [ ] CQRS pattern followed
- [ ] Repository pattern implemented
- [ ] All business logic from legacy preserved
- [ ] Authorization attributes applied
- [ ] 100% test coverage (validator + controller tests)
- [ ] All tests pass (no failing tests)
- [ ] All tests cleanup test data (no production impact)
- [ ] Stored procedures deployed to production
- [ ] Service registered in `Program.cs`

---

**Last Updated**: 2025-10-24  
**Modules Completed**: UnitMaster ✅, ItemCategoryMaster ✅  
**Next Module**: TBD

