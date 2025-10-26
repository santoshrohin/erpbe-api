# SmarterASP.NET Deployment Guide

## Issue Analysis

You're getting 404 errors on `http://santoshrohini-001-site44.qtempurl.com/` because:

1. **Runtime Issue**: SmarterASP.NET may not have .NET 9.0 installed yet (it's very new)
2. **Web.config Issue**: The configuration might not be optimized for shared hosting
3. **Environment Variables**: Production settings need to be configured
4. **Path Issue**: The application might not be deployed to the correct directory

## Solution Steps

### Step 1: Check .NET Runtime Support on SmarterASP.NET

**SmarterASP.NET currently supports:**
- .NET Core 3.1
- .NET 5.0
- .NET 6.0
- .NET 7.0
- .NET 8.0
- **❌ .NET 9.0 is NOT supported yet**

**ACTION REQUIRED**: We need to downgrade to .NET 8.0 (LTS)

### Step 2: Downgrade to .NET 8.0

I'll create the necessary configuration changes. .NET 8.0 is LTS (Long Term Support) and fully supported by SmarterASP.NET.

### Step 3: Create Production Configuration

We need:
1. Production `appsettings.Production.json`
2. Updated `web.config` for shared hosting
3. Proper publish profile for SmarterASP.NET

### Step 4: Deploy via FTP

SmarterASP.NET requires FTP deployment:
- **FTP Server**: ftp://santoshrohini-001-site44.qtempurl.com
- **Username**: santoshrohini-001-site44
- **Deploy To**: `/wwwroot/` folder

## Detailed Deployment Instructions

### A. Prepare Your Application

1. **Target .NET 8.0** (I'll update the project files)
2. **Create production settings**
3. **Publish in Release mode**
4. **Upload via FTP**

### B. SmarterASP.NET Control Panel Settings

1. **Login**: https://member5-4.smarterasp.net/cp/cp_screen
2. **Set .NET Version**:
   - Go to "Control Panel" → "Settings" → ".NET Version"
   - Select: **.NET 8.0**
   
3. **Enable ASP.NET Core**:
   - Go to "Control Panel" → "ASP.NET Core Module"
   - Ensure it's **Enabled**

4. **Set Application Pool**:
   - Should be set to "Integrated" mode
   - .NET CLR Version: "No Managed Code" (for .NET Core apps)

### C. Database Connection String

Your current connection string in `appsettings.json` is correct:
```
Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;
```

This should work from SmarterASP.NET shared hosting.

### D. Common Issues and Fixes

#### Issue 1: 404 Error
**Cause**: Wrong .NET version or app not in `/wwwroot/`
**Fix**: 
- Ensure .NET 8.0 is selected in control panel
- Deploy all files to `/wwwroot/` (not a subfolder)
- Ensure `web.config` is present

#### Issue 2: 500 Internal Server Error
**Cause**: Missing dependencies or configuration errors
**Fix**: 
- Enable detailed errors in `web.config` (I'll configure this)
- Check logs in control panel

#### Issue 3: Database Connection Fails
**Cause**: Firewall or incorrect connection string
**Fix**: 
- Ensure SQL Server allows external connections
- Test connection string from SmarterASP server

## Files I'll Create/Update

1. ✅ Downgrade all projects to .NET 8.0
2. ✅ Create `appsettings.Production.json`
3. ✅ Update `web.config` for shared hosting
4. ✅ Create SmarterASP publish profile
5. ✅ Create FTP deployment script

## Post-Deployment Testing

After deployment, test these URLs:

1. **Root**: http://santoshrohini-001-site44.qtempurl.com/
2. **Swagger**: http://santoshrohini-001-site44.qtempurl.com/swagger
3. **Health Check**: http://santoshrohini-001-site44.qtempurl.com/health
4. **API Endpoint**: http://santoshrohini-001-site44.qtempurl.com/api/Auth/login

## Important Notes

⚠️ **Swagger in Production**: By default, Swagger is disabled in production for security. I'll configure it to be enabled on your temp URL.

⚠️ **HTTPS**: SmarterASP.NET provides HTTPS on custom domains, but temp URLs (qtempurl.com) use HTTP only.

⚠️ **Logs**: Check application logs in SmarterASP control panel under "Log Manager"

## Let's Start the Migration

I'll now update your project to .NET 8.0 and create all necessary deployment files.

