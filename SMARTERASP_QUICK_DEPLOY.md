# SmarterASP.NET - Quick Deployment Guide

## 🎯 Quick Summary

Your app was getting 404 because it was targeting **.NET 9.0** which SmarterASP.NET doesn't support yet.

**✅ Fixed**: Downgraded to **.NET 8.0 (LTS)** - fully supported by SmarterASP.NET

## 🚀 Deploy in 3 Steps

### Step 1: Build and Publish

Run the PowerShell script:

```powershell
.\deploy-to-smarterasp.ps1
```

This will:
- ✅ Clean previous builds
- ✅ Restore packages
- ✅ Build in Release mode (.NET 8.0)
- ✅ Publish to `D:\Santosh\Work\Publish\SmarterASP`
- ✅ Verify all critical files

### Step 2: Upload via FTP

**Option A: Using FileZilla (Recommended)**

1. Download FileZilla: https://filezilla-project.org/
2. Open FileZilla
3. Enter connection details:
   - **Host**: `ftp://santoshrohini-001-site44.qtempurl.com`
   - **Username**: `santoshrohini-001-site44`
   - **Password**: [Your FTP password from SmarterASP control panel]
   - **Port**: `21`
4. Navigate to `/wwwroot/` on the remote server
5. Upload ALL files from `D:\Santosh\Work\Publish\SmarterASP`
6. **Important**: Select "Overwrite" when prompted

**Option B: Using FTP in Windows Explorer**

1. Open Windows Explorer
2. Type in address bar: `ftp://santoshrohini-001-site44.qtempurl.com`
3. Login with your credentials
4. Navigate to `wwwroot` folder
5. Copy all files from `D:\Santosh\Work\Publish\SmarterASP` to `wwwroot`

### Step 3: Configure SmarterASP.NET Control Panel

1. **Login**: https://member5-4.smarterasp.net/cp/cp_screen

2. **Set .NET Version**:
   - Go to: `Settings` → `.NET Version`
   - Select: **`.NET 8.0`**
   - Click: `Save`

3. **Enable ASP.NET Core Module**:
   - Go to: `ASP.NET Core Module`
   - Ensure: **Enabled**
   - If disabled, click `Enable`

4. **Recycle Application Pool**:
   - Go to: `Application Pool`
   - Click: **`Recycle`**
   - Wait 10-15 seconds

## ✅ Test Your Deployment

After deployment, test these URLs:

1. **Root API**: http://santoshrohini-001-site44.qtempurl.com/
2. **Swagger UI**: http://santoshrohini-001-site44.qtempurl.com/swagger
3. **Health Check**: http://santoshrohini-001-site44.qtempurl.com/health
4. **API Endpoint**: http://santoshrohini-001-site44.qtempurl.com/api/Auth/login

## 🐛 Troubleshooting

### Still Getting 404?

**Check 1: .NET Version**
- Ensure .NET 8.0 is selected in control panel
- Not .NET Core 3.1, 6.0, or 7.0

**Check 2: Files Uploaded Correctly**
- Verify `web.config` is in `/wwwroot/`
- Verify `ErpBE.API.dll` is in `/wwwroot/`
- Verify all files from publish folder are uploaded

**Check 3: Application Pool**
- Recycle the application pool again
- Wait 30 seconds
- Clear browser cache and retry

### Getting 500 Internal Server Error?

**Check Logs**:
1. Login to SmarterASP control panel
2. Go to: `Log Manager` → `Application Logs`
3. Look for recent errors
4. Also check: `/logs/stdout` files in FTP

**Enable Detailed Errors** (Already configured in web.config):
- The `web.config` is set to show detailed errors
- Visit the URL and you'll see the actual error message

### Database Connection Issues?

Your connection string is already correct:
```
Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;Encrypt=True;
```

If still having issues:
1. Verify database is accessible from SmarterASP.NET servers
2. Check firewall settings on SQL5111.site4now.net
3. Test connection using SmarterASP's SQL Server Management tool

## 📝 What Changed?

### Files Modified:

1. **All .csproj files** - Changed from `net9.0` to `net8.0`
2. **NuGet packages** - Updated to .NET 8.0 compatible versions
3. **web.config** - Optimized for shared hosting with detailed errors enabled
4. **appsettings.Production.json** - Created with production settings
5. **Program.cs** - Enabled Swagger in production (for testing)

### New Files:

1. **SmarterASP.pubxml** - Publish profile for SmarterASP.NET
2. **deploy-to-smarterasp.ps1** - Automated deployment script
3. **SMARTERASP_DEPLOYMENT_GUIDE.md** - Comprehensive guide

## 🔒 Security Notes

⚠️ **Swagger is currently enabled in production** for testing purposes.

After confirming the deployment works:
1. Set `EnableSwaggerInProduction` to `false` in `appsettings.Production.json`
2. Republish and redeploy

## 🌐 Production Domain Setup

When you have a custom domain:

1. Point domain to SmarterASP.NET
2. Update `AllowedHosts` in `appsettings.Production.json`
3. Disable Swagger (`EnableSwaggerInProduction: false`)
4. Update CORS policy to specific domain
5. Enable HTTPS (SmarterASP provides free SSL)

## 📞 Support

**SmarterASP.NET Support**:
- Website: https://www.smarterasp.net/support
- Email: support@smarterasp.net
- Knowledge Base: https://www.smarterasp.net/kb

**Common Issues**:
- Check: `COMMON_ISSUES_AND_FIXES.md` in your project root

---

## Quick Commands

```powershell
# Build and publish
.\deploy-to-smarterasp.ps1

# Or manually:
dotnet publish ErpBE.API/ErpBE.API.csproj -c Release -o D:\Santosh\Work\Publish\SmarterASP --framework net8.0

# Test locally before deploying
cd ErpBE.API
dotnet run --configuration Release
# Test at: https://localhost:7095/swagger
```

---

**Last Updated**: October 25, 2025  
**Target Framework**: .NET 8.0  
**Hosting Provider**: SmarterASP.NET  
**Deployment Method**: FTP

