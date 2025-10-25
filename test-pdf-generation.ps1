# Test script for Tax Invoice PDF Generation
# This script logs in, then calls the print endpoint

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  TAX INVOICE PDF GENERATION TEST" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "https://localhost:7032"
$apiUrl = "$baseUrl/api"

# Allow self-signed certificates (for testing)
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# Step 1: Login
Write-Host "Step 1: Logging in..." -ForegroundColor Yellow
$loginBody = @{
    username = "Mohan"
    password = "1234"
    companyId = 1
    financialYearCode = -2147483641
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/Auth/login" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    $token = $loginResponse.token
    Write-Host "SUCCESS: Login successful!" -ForegroundColor Green
    Write-Host "  User: $($loginResponse.username)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "ERROR: Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Get a Tax Invoice from database to test
Write-Host "Step 2: Fetching available Tax Invoices..." -ForegroundColor Yellow

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $invoicesResponse = Invoke-RestMethod -Uri "$apiUrl/TaxInvoice?CompanyId=1&PageNumber=1&PageSize=5" -Method GET -Headers $headers -ErrorAction Stop
    
    if ($invoicesResponse.data.Count -gt 0) {
        $firstInvoice = $invoicesResponse.data[0]
        Write-Host "SUCCESS: Found $($invoicesResponse.totalCount) invoices" -ForegroundColor Green
        Write-Host "  Testing with Invoice: $($firstInvoice.invoiceNumber)" -ForegroundColor Gray
        Write-Host "  Invoice Code: $($firstInvoice.invoiceCode)" -ForegroundColor Gray
        Write-Host "  Date: $($firstInvoice.invoiceDate)" -ForegroundColor Gray
        Write-Host "  Amount: Rs.$($firstInvoice.grandTotal)" -ForegroundColor Gray
        Write-Host ""
        
        $invoiceCode = $firstInvoice.invoiceCode
    } else {
        Write-Host "ERROR: No invoices found in the database!" -ForegroundColor Red
        Write-Host "  Please create a Tax Invoice first." -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "ERROR: Failed to fetch invoices: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Generate PDF
Write-Host "Step 3: Generating PDF..." -ForegroundColor Yellow

$outputPath = ".\TaxInvoice_$($firstInvoice.invoiceNumber.Replace('/', '_'))_Test.pdf"

try {
    $pdfUrl = "$apiUrl/TaxInvoice/$invoiceCode/print?companyId=1&copyType=0"
    Write-Host "  API: $pdfUrl" -ForegroundColor Gray
    
    $response = Invoke-WebRequest -Uri $pdfUrl -Method GET -Headers $headers -ErrorAction Stop
    
    [System.IO.File]::WriteAllBytes($outputPath, $response.Content)
    
    Write-Host "SUCCESS: PDF Generated Successfully!" -ForegroundColor Green
    Write-Host "  Saved to: $outputPath" -ForegroundColor Gray
    Write-Host "  Size: $([Math]::Round($response.Content.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host ""
    
    # Try to open the PDF
    Write-Host "Opening PDF..." -ForegroundColor Yellow
    Start-Process $outputPath
    Write-Host "SUCCESS: PDF opened in default viewer" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "ERROR: PDF generation failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "  Response: $responseBody" -ForegroundColor Red
    }
    exit 1
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  TEST COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
