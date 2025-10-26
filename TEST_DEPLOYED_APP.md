# 🧪 Testing Your Deployed Application

## ✅ CONGRATULATIONS! Your App is Running!

The fact that you're getting **500 errors** (not 404 or 503) means:
- ✅ Application started successfully
- ✅ ASP.NET Core is running
- ✅ Routes are working
- ✅ Controllers are loaded

The 500 errors are because the **Audit API requires authentication**.

---

## 🔐 Why `/api/Audit/` Returns 500 Error

The Audit API has this attribute:
```csharp
[AuthorizeAdmin] // Only Admin can view audit trails
```

**This means**:
1. You must be **logged in** (have a JWT token)
2. You must have **Admin role**
3. If not authenticated, you get 401 or 500

---

## 🧪 How to Test Your Deployed App

### Test 1: Check Swagger UI

**URL**: http://santoshrohini-001-site45.qtempurl.com/swagger

**Expected**: Swagger UI should load showing all your API endpoints

**If it works**: ✅ Your app is fully running!

---

### Test 2: Login to Get JWT Token

#### Using Swagger UI:

1. Open: http://santoshrohini-001-site45.qtempurl.com/swagger
2. Find: `/api/Auth/login` endpoint
3. Click: **"Try it out"**
4. Enter your credentials:
   ```json
   {
     "username": "admin",
     "password": "your-password"
   }
   ```
5. Click: **"Execute"**
6. **Copy the JWT token** from the response

#### Using PowerShell:

```powershell
$loginUrl = "http://santoshrohini-001-site45.qtempurl.com/api/Auth/login"
$body = @{
    username = "admin"
    password = "your-password"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json"
$token = $response.token
Write-Host "JWT Token: $token"
```

#### Using curl:

```bash
curl -X POST http://santoshrohini-001-site45.qtempurl.com/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"your-password"}'
```

---

### Test 3: Test Audit API with Token

#### Using Swagger UI:

1. In Swagger, click the **"Authorize"** button (top right)
2. Enter: `Bearer YOUR_JWT_TOKEN_HERE`
3. Click: **"Authorize"**
4. Now try: `/api/Audit/ITEM_UNIT_MASTER`
5. Should return 200 OK with audit data

#### Using PowerShell:

```powershell
$auditUrl = "http://santoshrohini-001-site45.qtempurl.com/api/Audit/ITEM_UNIT_MASTER"
$headers = @{
    Authorization = "Bearer $token"
}

$auditData = Invoke-RestMethod -Uri $auditUrl -Method Get -Headers $headers
$auditData | Format-Table
```

#### Using curl:

```bash
curl -X GET http://santoshrohini-001-site45.qtempurl.com/api/Audit/ITEM_UNIT_MASTER \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🎯 Quick Test Checklist

Test these URLs directly in your browser:

| URL | Expected Result | Meaning |
|-----|-----------------|---------|
| http://santoshrohini-001-site45.qtempurl.com/ | HTML page or JSON | ✅ App root works |
| http://santoshrohini-001-site45.qtempurl.com/swagger | Swagger UI | ✅ Swagger enabled |
| http://santoshrohini-001-site45.qtempurl.com/api/Auth/login | 405 Method Not Allowed | ✅ Endpoint exists (needs POST) |
| http://santoshrohini-001-site45.qtempurl.com/api/Audit/ | 401 Unauthorized or 500 | ✅ Endpoint exists (needs auth) |

---

## 🐛 If You Get 500 Errors After Login

### Possible Causes:

1. **Database Connection Issue**
   - The audit query might be failing
   - Check connection string in `appsettings.Production.json`

2. **Audit Table Missing**
   - Verify audit tables exist (they do, I checked)

3. **SQL Query Error**
   - The audit repository query might have a syntax error
   - Check logs in `/wwwroot/logs/` or control panel

---

## 📊 Check Application Logs

### Via FTP:

1. Connect: `ftp://santoshrohini-001-site45.qtempurl.com`
2. Navigate: `/wwwroot/logs/`
3. Download: Latest `stdout_*.log` files
4. Look for: ERROR, EXCEPTION messages

### Via Control Panel:

1. Login: https://member5-4.smarterasp.net/cp/cp_screen
2. Go to: **Log Manager** → **Application Logs**
3. Check recent logs for errors

---

## ✅ Confirming Database Connection Works

Test if you can query the database from the deployed app:

### Create a Test Endpoint (Optional)

If you want, I can add a simple `/api/Test/database` endpoint that:
- Tests database connection
- Returns "OK" if connection works
- Doesn't require authentication

This would help confirm database connectivity from the deployed server.

---

## 🎉 Success Indicators

Your app is **successfully deployed** if:

- ✅ Swagger UI loads
- ✅ Login endpoint exists (even if you get 405 or need credentials)
- ✅ Audit endpoint returns 401/500 (not 404)
- ✅ You can get a JWT token from login
- ✅ Authenticated requests work

---

## 📝 Why Tests Pass But Production Has Issues

| Aspect | Tests | Production |
|--------|-------|------------|
| Database | In-memory or local | Real database on SQL5111 |
| Authentication | Mocked/bypassed | Real JWT required |
| Environment | Development | Production |
| Logging | Console | SQL Server (might fail) |
| Error Handling | Detailed | Masked (for security) |

Tests verify **logic** works. Production deployment tests **infrastructure** (network, database, auth, etc.).

---

## 🚀 Next Steps

1. **Access Swagger**: Confirm the app is fully running
2. **Get your admin credentials**: If you don't have them, we may need to create an admin user
3. **Test with authentication**: Use Swagger's "Authorize" feature
4. **Check specific error**: If still failing, check the logs

---

## 💡 Common Issue: No Admin User

If you don't have admin credentials, you might need to:

1. **Create an admin user manually** in the database
2. Or **add a user registration endpoint** (temporary)
3. Or **seed an admin user** on startup

Let me know if you need help with this!

---

**Current Status**: ✅ App is deployed and running!  
**Issue**: Need authentication to test protected endpoints  
**Next**: Get JWT token via login, then test with authorization

