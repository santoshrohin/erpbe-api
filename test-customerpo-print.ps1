# Test Customer PO Print Endpoints
# This script tests the newly implemented Customer PO print functionality

$ErrorActionPreference = "Continue"

$baseUrl = "http://localhost:5136"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Customer PO Print Endpoints" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login to get auth token
Write-Host "[1] Logging in..." -ForegroundColor Yellow
$loginBody = @{
    userName = "mohan"
    password = "390-400-410-420"
    companyId = 1
    financialYearCode = 1
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "  Success! Token received" -ForegroundColor Green
} catch {
    Write-Host "  Failed to login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Get all Customer POs to find a valid PoCode
Write-Host "[2] Fetching Customer POs..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $customerPosResponse = Invoke-RestMethod -Uri "$baseUrl/api/CustomerPo?CompanyId=1&IsActive=true&PageNumber=1&PageSize=10" -Method GET -Headers $headers
    
    if ($customerPosResponse.data.Count -eq 0) {
        Write-Host "  No Customer POs found in the database" -ForegroundColor Yellow
        Write-Host "  Skipping print tests..." -ForegroundColor Yellow
        exit 0
    }
    
    $firstPo = $customerPosResponse.data[0]
    $poCode = $firstPo.poCode
    $companyId = $firstPo.companyId
    
    Write-Host "  Found $($customerPosResponse.totalCount) Customer PO(s)" -ForegroundColor Green
    Write-Host "  Testing with PoCode: $poCode, CompanyId: $companyId" -ForegroundColor Cyan
} catch {
    Write-Host "  Failed to fetch Customer POs: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Test Single PO Print (Original)
Write-Host "[3] Testing Single PO Print (Original - copyType=0)..." -ForegroundColor Yellow
try {
    $printResponse = Invoke-WebRequest -Uri "$baseUrl/api/CustomerPo/$poCode/print?companyId=$companyId&copyType=0" -Method GET -Headers $headers
    
    if ($printResponse.StatusCode -eq 200) {
        $pdfFileName = "CustomerPO_${poCode}_Original.pdf"
        [System.IO.File]::WriteAllBytes($pdfFileName, $printResponse.Content)
        Write-Host "  Success! PDF saved: $pdfFileName" -ForegroundColor Green
        Write-Host "  File size: $([math]::Round($printResponse.Content.Length / 1KB, 2)) KB" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  Failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "  Response: $responseBody" -ForegroundColor Gray
    }
}

Write-Host ""

# Step 4: Test Single PO Print (Duplicate)
Write-Host "[4] Testing Single PO Print (Duplicate - copyType=1, should generate 2 pages)..." -ForegroundColor Yellow
try {
    $printResponse = Invoke-WebRequest -Uri "$baseUrl/api/CustomerPo/$poCode/print?companyId=$companyId&copyType=1" -Method GET -Headers $headers
    
    if ($printResponse.StatusCode -eq 200) {
        $pdfFileName = "CustomerPO_${poCode}_Duplicate.pdf"
        [System.IO.File]::WriteAllBytes($pdfFileName, $printResponse.Content)
        Write-Host "  Success! PDF saved: $pdfFileName" -ForegroundColor Green
        Write-Host "  File size: $([math]::Round($printResponse.Content.Length / 1KB, 2)) KB" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Step 5: Test Single PO Print (Triplicate)
Write-Host "[5] Testing Single PO Print (Triplicate - copyType=2, should generate 3 pages)..." -ForegroundColor Yellow
try {
    $printResponse = Invoke-WebRequest -Uri "$baseUrl/api/CustomerPo/$poCode/print?companyId=$companyId&copyType=2" -Method GET -Headers $headers
    
    if ($printResponse.StatusCode -eq 200) {
        $pdfFileName = "CustomerPO_${poCode}_Triplicate.pdf"
        [System.IO.File]::WriteAllBytes($pdfFileName, $printResponse.Content)
        Write-Host "  Success! PDF saved: $pdfFileName" -ForegroundColor Green
        Write-Host "  File size: $([math]::Round($printResponse.Content.Length / 1KB, 2)) KB" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Step 6: Test Batch Print (if we have multiple POs)
if ($customerPosResponse.data.Count -gt 1) {
    Write-Host "[6] Testing Batch Print (first 2 POs)..." -ForegroundColor Yellow
    
    $po1 = $customerPosResponse.data[0]
    $po2 = $customerPosResponse.data[1]
    
    $batchBody = @{
        companyId = $companyId
        pos = @(
            @{
                poCode = $po1.poCode
                copyType = 0
            },
            @{
                poCode = $po2.poCode
                copyType = 1
            }
        )
    } | ConvertTo-Json -Depth 3
    
    try {
        $batchResponse = Invoke-WebRequest -Uri "$baseUrl/api/CustomerPo/print-batch" -Method POST -Body $batchBody -ContentType "application/json" -Headers $headers
        
        if ($batchResponse.StatusCode -eq 200) {
            $pdfFileName = "CustomerPOs_Batch.pdf"
            [System.IO.File]::WriteAllBytes($pdfFileName, $batchResponse.Content)
            Write-Host "  Success! Batch PDF saved: $pdfFileName" -ForegroundColor Green
            Write-Host "  File size: $([math]::Round($batchResponse.Content.Length / 1KB, 2)) KB" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "  Failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "  Response: $responseBody" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "[6] Skipping Batch Print (only 1 PO available)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Generated PDF files are in the current directory" -ForegroundColor Cyan

