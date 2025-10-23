# Visual Studio Code Coverage - Easy Setup

## 🎯 Best Option: Visual Studio Extensions

### Option 1: Fine Code Coverage (FREE & Best) ⭐

**Install from Visual Studio:**
1. Go to **Extensions** → **Manage Extensions**
2. Search for **"Fine Code Coverage"**
3. Click **Download** and restart Visual Studio
4. Done! Coverage highlights appear automatically when you run tests

**Features:**
- ✅ **Real-time inline coverage** - See covered/uncovered lines in your code
- ✅ **Automatic** - No scripts needed
- ✅ **Color highlights** - Green = covered, Red = not covered
- ✅ **Coverage window** - Shows percentages per file/class
- ✅ **Works with xUnit, NUnit, MSTest**
- ✅ **FREE**

**How to Use:**
1. Open **Test Explorer** (Test → Test Explorer)
2. Run your tests
3. Coverage highlights appear automatically in your code!
4. View **Fine Code Coverage** window (View → Other Windows → Fine Code Coverage)

---

### Option 2: Visual Studio Enterprise (Built-in)

If you have **Visual Studio Enterprise** edition:

1. Run tests with coverage: **Test → Analyze Code Coverage → All Tests**
2. View **Code Coverage Results** window
3. Double-click any assembly to see detailed coverage
4. Coverage highlights appear inline

**Note:** This requires Visual Studio Enterprise license (not free)

---

### Option 3: Coverlet + ReportGenerator Integration

**Install NuGet Package in Test Project:**
```xml
<ItemGroup>
  <PackageReference Include="coverlet.msbuild" Version="6.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

**Configure in Test Project (.csproj):**
```xml
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <CoverletOutput>./coverage/</CoverletOutput>
  <ExcludeByFile>**/*.Designer.cs</ExcludeByFile>
</PropertyGroup>
```

Then install **Coverage Gutters** extension:
- Extensions → Manage Extensions
- Search for "Coverage Gutters"
- Install and restart

---

## 🚀 Quick Comparison

| Solution | Cost | Inline Highlights | Auto Updates | Ease of Use |
|----------|------|-------------------|--------------|-------------|
| **Fine Code Coverage** | FREE | ✅ Yes | ✅ Yes | ⭐⭐⭐⭐⭐ |
| VS Enterprise | $$$$ | ✅ Yes | ✅ Yes | ⭐⭐⭐⭐ |
| Coverage Gutters | FREE | ✅ Yes | ⚠️ Manual | ⭐⭐⭐ |
| PowerShell Scripts | FREE | ❌ No | ❌ No | ⭐⭐ |

---

## ✅ Recommended Setup (Fine Code Coverage)

### Step 1: Install Extension
1. **Extensions** → **Manage Extensions**
2. Search: **"Fine Code Coverage"**
3. Install (by Fortune Ngwenya)

### Step 2: Configure (Optional)
Create `.finecodecoverage/config.xml` in solution root:
```xml
<?xml version="1.0" encoding="utf-8"?>
<FineCodeCoverage>
  <Enabled>True</Enabled>
  <IncludeTestAssembly>False</IncludeTestAssembly>
  <ExcludeAssemblies>
    <Assembly>*.Tests</Assembly>
  </ExcludeAssemblies>
  <ExcludeByFile>
    <Exclude>**/*.Designer.cs</Exclude>
    <Exclude>**/Migrations/**</Exclude>
  </ExcludeByFile>
</FineCodeCoverage>
```

### Step 3: Use It
1. Open **Test Explorer** (Test → Test Explorer)
2. Run tests (click ▶ Run All)
3. Watch coverage highlights appear in your code!
4. Open coverage window: **View** → **Other Windows** → **Fine Code Coverage**

---

## 📊 What You'll See

### In Your Code Editor:
```csharp
public class UserService  // ← Coverage bar (green/red)
{
    public void CreateUser(...)  // 🟢 Green = covered
    {
        // Code here...
    }
    
    public void DeleteUser(...)  // 🔴 Red = NOT covered
    {
        // Code here - needs tests!
    }
}
```

### In Coverage Window:
```
Assembly              Line %   Branch %
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
ErpBE.Application     78.5%    65.2%
ErpBE.Domain          85.3%    72.1%
ErpBE.Infrastructure  68.9%    58.4%
```

---

## 🎯 Why Fine Code Coverage is Best

1. ✅ **Zero configuration** - Works out of the box
2. ✅ **Inline highlights** - See coverage while coding
3. ✅ **Real-time** - Updates as you run tests
4. ✅ **FREE** - No license needed
5. ✅ **Works with all test frameworks** - xUnit, NUnit, MSTest
6. ✅ **Visual Studio 2019/2022** - Modern versions

---

## 🔧 Troubleshooting

### Coverage not showing?
1. Check **Output** window → Select "Fine Code Coverage"
2. Verify tests are running successfully
3. Ensure `coverlet.collector` is installed in test project

### Want to exclude files?
Edit `config.xml`:
```xml
<ExcludeByFile>
  <Exclude>**/*.Designer.cs</Exclude>
  <Exclude>**/Program.cs</Exclude>
  <Exclude>**/Migrations/**</Exclude>
</ExcludeByFile>
```

---

## 📝 Bottom Line

**You DON'T need PowerShell scripts!** 

Just install **Fine Code Coverage** extension and run your tests from Test Explorer. Coverage highlights will appear automatically in your code.

**PowerShell scripts are optional** - useful for CI/CD or generating reports for team reviews, but not needed for daily development.

