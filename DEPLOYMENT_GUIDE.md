# 🚀 ERP API Deployment Guide for SmarterASP.net

## 📋 Prerequisites

1. **SmarterASP.net Account** with ASP.NET hosting plan
2. **SQL Server Database** access details
3. **FTP/Web Deploy** access to your hosting account

## 🔧 Step 1: Database Setup

### 1.1 Create Logs Table
1. Log into your SmarterASP.net control panel
2. Go to **SQL Server** section
3. Open **SQL Management Studio** or use the web-based query tool
4. Run the script: `ErpBE.Infrastructure/Scripts/Logs_Table_Setup.sql`
5. Replace `YOUR_DATABASE_NAME` with your actual database name

### 1.2 Run Existing Scripts
Make sure you've also run:
- `Complete_Role_Setup.sql` (for user roles)
- `Audit_Database_Setup.sql` (for audit tables)

## 🔧 Step 2: Get Connection Details

From your SmarterASP.net control panel, note down:
- **Server Name**: `sqlXXX.smarterasp.net` (or similar)
- **Database Name**: Your database name
- **Username**: Your SQL username
- **Password**: Your SQL password

## 🔧 Step 3: Configure Application

### 3.1 Update Connection String
1. Copy `appsettings.Production.Template.json` to `appsettings.Production.json`
2. Replace the placeholder values:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=sqlXXX.smarterasp.net;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASS;TrustServerCertificate=true;"
   }
   ```

### 3.2 Update JWT Secret
Replace `YOUR_PRODUCTION_SECRET_KEY_HERE` with a strong secret key (at least 32 characters).

### 3.3 Update Serilog Connection
Update the Serilog connection string in `appsettings.Production.json` with the same database details.

## 🔧 Step 4: Build and Publish

### 4.1 Build for Production
```bash
dotnet publish -c Release -o ./publish
```

### 4.2 Files to Deploy
Upload these files/folders to your SmarterASP.net hosting:
- `publish/` folder contents
- `web.config`
- `appsettings.Production.json` (rename from template)

## 🔧 Step 5: Deploy to SmarterASP.net

### 5.1 Using FTP
1. Connect to your FTP server
2. Upload all files to the root directory
3. Ensure `web.config` is in the root

### 5.2 Using Web Deploy (Recommended)
1. Use Visual Studio's **Publish** feature
2. Select **Web Deploy** as the publish method
3. Enter your SmarterASP.net credentials
4. Deploy directly to the server

## 🔧 Step 6: Configure IIS (if needed)

SmarterASP.net usually handles this automatically, but if you need to configure:

1. **Application Pool**: Set to **No Managed Code** (for .NET Core)
2. **Default Document**: Ensure `web.config` is present
3. **Error Pages**: Custom error pages are configured in `web.config`

## 🔧 Step 7: Test Deployment

### 7.1 Test API Endpoints
1. **Health Check**: `https://yourdomain.com/api/Login` (should return 400 for missing body)
2. **Swagger**: `https://yourdomain.com/` (should show Swagger UI)
3. **Logs Viewer**: `https://yourdomain.com/logs` (requires authentication)

### 7.2 Test Logging
1. Make some API calls
2. Check the `Logs` table in your database
3. Verify logs are being written

## 🔧 Step 8: Monitor and Maintain

### 8.1 View Logs
- **Web Interface**: `https://yourdomain.com/logs`
- **Database**: Query the `Logs` table directly
- **Statistics**: Use `/api/Logs/statistics` endpoint

### 8.2 Log Cleanup
- **Automatic**: Set up a scheduled task to call `/api/Logs/cleanup`
- **Manual**: Use the web interface or API endpoint

## 🔧 Step 9: Security Considerations

### 9.1 Production Secrets
- ✅ Use strong JWT secrets
- ✅ Use HTTPS only
- ✅ Restrict database access
- ✅ Regular security updates

### 9.2 Log Security
- ✅ Only Admin users can view logs
- ✅ Sensitive data should not be logged
- ✅ Regular log cleanup

## 🔧 Step 10: Troubleshooting

### 10.1 Common Issues

**Issue**: Application won't start
- **Solution**: Check `web.config` and connection strings

**Issue**: Logs not appearing
- **Solution**: Verify database connection and table creation

**Issue**: Authentication not working
- **Solution**: Check JWT settings and database user roles

**Issue**: Swagger not loading
- **Solution**: Ensure you're in Development mode or configure for Production

### 10.2 Log Locations
- **Application Logs**: Database `Logs` table
- **IIS Logs**: SmarterASP.net control panel
- **Error Logs**: Check `logs/` folder on server

## 📊 Monitoring Dashboard

Once deployed, you can access:
- **API Documentation**: `https://yourdomain.com/`
- **Logs Viewer**: `https://yourdomain.com/logs`
- **Health Check**: `https://yourdomain.com/api/Login`

## 🎯 Next Steps

1. **Set up monitoring** alerts for errors
2. **Configure log rotation** to prevent database bloat
3. **Set up backups** for your database
4. **Monitor performance** using the statistics endpoint

## 📞 Support

If you encounter issues:
1. Check the logs in your database
2. Review SmarterASP.net documentation
3. Contact SmarterASP.net support for hosting issues
4. Check the application logs for detailed error information

---

**Happy Deploying! 🚀**
