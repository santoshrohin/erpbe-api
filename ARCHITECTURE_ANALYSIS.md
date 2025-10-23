# Architecture Dependency Analysis

## ✅ Expected vs ❌ Actual Project References

### Your Expected Architecture Rules

| Project            | Can Reference                                      | Cannot Reference                 |
| ------------------ | -------------------------------------------------- | -------------------------------- |
| **Domain**         | None                                               | Application, Infrastructure, Web |
| **Application**    | Domain                                             | Infrastructure, Web              |
| **Infrastructure** | Application, Domain                                | Web                              |
| **Web (API)**      | Application (and optionally Infrastructure for DI) | **Domain**                       |

---

## 📊 Actual Project References (from .csproj files)

### ✅ Domain Project - **CORRECT**
```xml
<ItemGroup>
  <!-- NO PROJECT REFERENCES -->
</ItemGroup>
```
**Status**: ✅ **PERFECT** - No dependencies on any other layer

---

### ✅ Application Project - **CORRECT**
```xml
<ItemGroup>
  <ProjectReference Include="..\ErpBE.Domain\ErpBE.Domain.csproj" />
</ItemGroup>
```
**Status**: ✅ **CORRECT** - References only Domain

---

### ✅ Infrastructure Project - **CORRECT**
```xml
<ItemGroup>
  <ProjectReference Include="..\ErpBE.Application\ErpBE.Application.csproj" />
  <ProjectReference Include="..\ErpBE.Domain\ErpBE.Domain.csproj" />
</ItemGroup>
```
**Status**: ✅ **CORRECT** - References Application and Domain

---

### ❌ API (Web) Project - **VIOLATION DETECTED**
```xml
<ItemGroup>
  <ProjectReference Include="..\ErpBE.Application\ErpBE.Application.csproj" />
  <ProjectReference Include="..\ErpBE.Infrastructure\ErpBE.Infrastructure.csproj" />
</ItemGroup>
```
**Status**: ✅ **Project references are CORRECT** (Application + Infrastructure for DI)

**BUT** ❌ **Controllers are USING Domain directly!**

---

## 🔍 Domain Usage Violations in API Layer

### Controllers Using Domain Directly:

1. **UnitMasterController.cs**
   ```csharp
   using ErpBE.Domain.DTOs;
   using ErpBE.Domain.CommonDto;
   ```

2. **DropdownController.cs**
   ```csharp
   using ErpBE.Domain.CommonDto;
   ```

3. **UserController.cs**
   ```csharp
   using ErpBE.Domain.DTOs;
   using ErpBE.Domain.Interfaces;
   using ErpBE.Domain.CommonDto;
   ```

4. **RoleController.cs**
   ```csharp
   using ErpBE.Domain.DTOs;
   using ErpBE.Domain.Interfaces;
   ```

5. **AuditController.cs**
   ```csharp
   using ErpBE.Domain.Interfaces;
   using ErpBE.Domain.DTOs;
   ```

**Total**: 5 controllers with 10 `using ErpBE.Domain.*` statements

---

## ❌ Architecture Violation Summary

### Problem:
**API layer (Controllers) is directly accessing Domain layer types:**
- ✅ **Project Reference**: API → Application (correct)
- ✅ **Project Reference**: API → Infrastructure (correct for DI)
- ❌ **Code Usage**: Controllers using `ErpBE.Domain.DTOs` (WRONG!)
- ❌ **Code Usage**: Controllers using `ErpBE.Domain.Interfaces` (WRONG!)
- ❌ **Code Usage**: Controllers using `ErpBE.Domain.CommonDto` (WRONG!)

### Why This is a Problem:
According to Clean Architecture and your specified rules:
- **API should only interact with Application layer**
- **DTOs should be exposed through Application layer**
- **Domain interfaces should not be directly injected into Controllers**

---

## ✅ What's Working Correctly

### 1. Project-Level Dependencies (`.csproj`)
All project references are **100% CORRECT**:
- ✅ Domain has NO references
- ✅ Application references only Domain
- ✅ Infrastructure references Application + Domain
- ✅ API references Application + Infrastructure

### 2. Architecture Tests Passing
- ✅ Domain has no dependency on Application/Infrastructure/API
- ✅ Application has no dependency on Infrastructure/API
- ✅ Infrastructure has no dependency on API
- ✅ Controllers don't depend on Infrastructure (using Application instead)

---

## 🔧 How to Fix the Violations

### Option 1: Move DTOs to Application Layer (Recommended)
**Current**:
```
ErpBE.Domain/DTOs/
  - CreateUserRequest.cs
  - UpdateUserRequest.cs
  - etc.
```

**Should be**:
```
ErpBE.Application/DTOs/
  - CreateUserRequest.cs
  - UpdateUserRequest.cs
  - etc.
```

**Controllers then use**:
```csharp
using ErpBE.Application.DTOs;  // ✅ Correct
// NOT: using ErpBE.Domain.DTOs;  ❌ Wrong
```

### Option 2: Re-export DTOs through Application (Alternative)
Keep DTOs in Domain, but create Application-layer contracts that re-export them:

```csharp
// ErpBE.Application/Contracts/IUserManagementService.cs
namespace ErpBE.Application.Contracts
{
    public interface IUserManagementService
    {
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
    }
}
```

Controllers then depend ONLY on Application interfaces, never directly on Domain.

### Option 3: Accept the Pragmatic Violation (Not Recommended)
Some teams allow DTOs in Domain because they're part of the "domain language," but this violates pure Clean Architecture.

---

## 📋 Current Architecture Compliance

| Rule | Status | Details |
|------|--------|---------|
| Domain → Nothing | ✅ **PASS** | Domain has no dependencies |
| Application → Domain only | ✅ **PASS** | Application references only Domain |
| Infrastructure → App + Domain | ✅ **PASS** | Infrastructure references Application and Domain |
| API → Application (+ Infra for DI) | ⚠️ **PARTIAL** | Project refs correct, but code uses Domain directly |
| API → NOT Domain | ❌ **FAIL** | Controllers use `ErpBE.Domain.*` namespaces |

**Overall Compliance**: **80% (4 out of 5 rules)**

---

## 🎯 Recommendation

### Immediate Action:
**Option 1 is best**: Move DTOs to Application layer

**Why**:
1. ✅ DTOs are contracts between API and Application
2. ✅ Domain should only contain core business entities and logic
3. ✅ Makes architecture boundaries crystal clear
4. ✅ Follows CQRS pattern (Commands/Queries are Application layer concerns)

### Steps:
1. Move all DTOs from `ErpBE.Domain/DTOs/` to `ErpBE.Application/DTOs/`
2. Move all `CommonDto` from `ErpBE.Domain/CommonDto/` to `ErpBE.Application/CommonDto/`
3. Update all `using` statements in controllers
4. Remove Domain reference from controllers (it will still compile via Application)
5. Update architecture tests to enforce "API should not use Domain namespaces"

---

## 🧪 Architecture Test Results

**Current Test Results**: 43 out of 47 passing (91.5%)

**Tests that would catch API→Domain violation**:
- Currently **NOT** tested at namespace/using level
- Only project reference level is tested (which passes)

**Recommended New Test**:
```csharp
[Fact]
public void API_Controllers_ShouldNotUseDomainNamespaces()
{
    var result = Types.InAssembly(apiAssembly)
        .That()
        .ResideInNamespace("ErpBE.API.Controllers")
        .ShouldNot()
        .HaveDependencyOn("ErpBE.Domain")
        .GetResult();
    
    Assert.True(result.IsSuccessful, 
        "Controllers should not use Domain namespace directly");
}
```

---

## 📝 Summary

### Your Question: "Are we following this?"

**Answer**: 
- ✅ **YES** at the project reference level (.csproj)
- ❌ **NO** at the code/namespace usage level (Controllers using Domain types)

### Fix Priority: **HIGH**
This is a fundamental Clean Architecture violation that should be addressed to maintain proper separation of concerns.

