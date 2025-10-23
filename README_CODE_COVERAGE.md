# Code Coverage - Choose Your Method

## 🎯 Three Ways to View Coverage

### ⭐ Method 1: Visual Studio Extension (RECOMMENDED)

**Install:** [Fine Code Coverage Extension](https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage2022)

**Pros:**
- ✅ **Real-time inline coverage** - See coverage while coding
- ✅ **Zero configuration** - Already set up in this project
- ✅ **Automatic** - Just run tests from Test Explorer
- ✅ **Visual** - Green/red bars in your code
- ✅ **FREE**

**How to use:**
1. Install extension (one-time, 2 minutes)
2. Run tests from Test Explorer
3. Coverage appears automatically!

**See:** `INSTALL_FINE_CODE_COVERAGE.md` for step-by-step guide

---

### Method 2: PowerShell Script (For CI/CD)

**Use:** `.\Generate-Coverage.ps1`

**Pros:**
- ✅ Good for automated builds
- ✅ Generates HTML reports for team sharing
- ✅ Works without Visual Studio

**Cons:**
- ❌ Manual process
- ❌ No inline highlighting
- ❌ Slower workflow

**When to use:**
- CI/CD pipelines
- Generating reports for stakeholders
- Command-line workflows

---

### Method 3: Visual Studio Enterprise (Built-in)

**Requires:** Visual Studio Enterprise license ($$$)

**Pros:**
- ✅ Built directly into VS
- ✅ Professional-grade features

**Cons:**
- ❌ Expensive license required
- ❌ Fine Code Coverage is FREE and just as good

---

## 🏆 Winner: Fine Code Coverage Extension

**Why?**
- ✅ FREE (vs VS Enterprise = $$$)
- ✅ Easier than PowerShell scripts
- ✅ Real-time feedback while coding
- ✅ Works with your existing setup (packages already installed!)

---

## 📦 What's Already Set Up

Your `ErpBE.Tests` project **already has** the necessary packages:

```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
<PackageReference Include="coverlet.msbuild" Version="6.0.2" />
```

**You only need to:**
1. Install the Visual Studio extension (2 minutes)
2. Run your tests
3. Done!

---

## 🚀 Quick Start

### For Daily Development (Recommended):
```
1. Install "Fine Code Coverage" extension in Visual Studio
2. Open Test Explorer (Ctrl+E, T)
3. Run All Tests
4. Watch coverage highlights appear in your code!
```

### For CI/CD or Reports:
```powershell
.\Generate-Coverage.ps1
```

---

## 📖 Documentation

- **Extension Setup:** `INSTALL_FINE_CODE_COVERAGE.md`
- **VS Extension Details:** `VISUAL_STUDIO_COVERAGE_SETUP.md`
- **PowerShell Guide:** `CODE_COVERAGE_GUIDE.md`
- **Testing Overview:** `TESTING_AND_COVERAGE_SUMMARY.md`

---

## 💡 Bottom Line

**You DON'T need PowerShell scripts for daily work!**

Just install the **Fine Code Coverage** extension and run your tests. Coverage will appear automatically as you code.

**PowerShell scripts are optional** - useful for CI/CD, but not needed for development.

---

**Recommended Next Step:** Read `INSTALL_FINE_CODE_COVERAGE.md` for 2-minute setup! ✨

