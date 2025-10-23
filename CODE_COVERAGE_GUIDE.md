# Code Coverage Guide

This document explains how to generate and view code coverage reports for the ERP Backend API.

## 📊 What is Code Coverage?

Code coverage shows which lines of your code are executed by your tests. It helps you:
- ✅ Identify untested code
- ✅ Improve test quality
- ✅ Find dead code
- ✅ Track testing progress

## 🎯 Quick Start

### Option 1: Simple PowerShell Script (Recommended)

```powershell
.\Generate-Coverage.ps1
```

This will:
1. Run all tests (unit + integration)
2. Collect coverage data
3. Generate HTML report
4. Automatically open it in your browser

### Option 2: Manual Commands

```powershell
# Run tests with coverage collection
dotnet test ErpBE.Tests --collect:"XPlat Code Coverage"

# Find the coverage file
$coverageFile = Get-ChildItem -Path "TestResults" -Filter "*.cobertura.xml" -Recurse | Select-Object -First 1

# Generate HTML report
reportgenerator "-reports:$($coverageFile.FullName)" "-targetdir:coverage-report" "-reporttypes:Html"

# Open the report
Start-Process "coverage-report\index.html"
```

## 📖 Understanding the Report

The HTML report shows:

### Summary Page
- **Line Coverage**: Percentage of code lines executed
- **Branch Coverage**: Percentage of decision branches tested  
- **Method Coverage**: Percentage of methods called

### Detailed View
- 🟢 **Green**: Code covered by tests
- 🔴 **Red**: Code NOT covered by tests  
- 🟡 **Yellow**: Partially covered (some branches)

## 🎯 Coverage Goals

| Metric | Minimum | Target | Excellent |
|--------|---------|--------|-----------|
| Line Coverage | 60% | 75% | 85%+ |
| Branch Coverage | 50% | 70% | 80%+ |

## 📁 Generated Files

- `coverage-report/` - HTML reports (open `index.html`)
- `TestResults/` - Raw coverage data (.cobertura.xml)

**.gitignore** automatically excludes these from version control.

## 🔧 Advanced Options

### Generate Multiple Report Formats

```powershell
reportgenerator `
    "-reports:TestResults/**/*.cobertura.xml" `
    "-targetdir:coverage-report" `
    "-reporttypes:Html;Badges;Cobertura"
```

### Filter Specific Projects

```powershell
dotnet test ErpBE.Tests `
    --collect:"XPlat Code Coverage" `
    --filter "FullyQualifiedName~UnitMaster"
```

### View Coverage in Visual Studio

1. Install **Fine Code Coverage** extension
2. Run tests from Test Explorer
3. Coverage highlights appear inline in code

## 🧹 Cleanup

Coverage files are gitignored and safe to delete:

```powershell
Remove-Item coverage-report -Recurse -Force
Remove-Item TestResults -Recurse -Force
```

## 🆘 Troubleshooting

### "No coverage file found"
- Ensure tests ran successfully
- Check `TestResults/` folder exists
- Verify coverlet.collector is installed

### "Report Generator not found"
Install globally:
```powershell
dotnet tool install --global dotnet-reportgenerator-globaltool
```

### Low Coverage Numbers
- Add more unit tests for business logic
- Integration tests count toward coverage
- Focus on critical paths first

## 📚 Additional Resources

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Docs](https://github.com/danielpalme/ReportGenerator)
- [xUnit Best Practices](https://xunit.net/docs/getting-started/netcore/cmdline)

## 🎓 Best Practices

1. **Run coverage regularly** - At least before commits
2. **Focus on quality** - 100% coverage ≠ good tests
3. **Test business logic** - Prioritize critical code
4. **Ignore generated code** - Migrations, designers, etc.
5. **Track trends** - Coverage should improve over time

---

**Last Updated**: October 2025  
**Maintainer**: Development Team

