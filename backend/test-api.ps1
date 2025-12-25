# Script test API cho MyGarden Backend

Write-Host "=== TEST ĐĂNG KÝ ===" -ForegroundColor Green
$registerBody = @{
    email = "test@test.com"
    password = "123456"
} | ConvertTo-Json

$registerResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" `
    -Method POST `
    -ContentType "application/json" `
    -Body $registerBody

Write-Host "Kết quả đăng ký:" -ForegroundColor Yellow
$registerResponse | ConvertTo-Json

Write-Host "`n=== TEST ĐĂNG NHẬP ===" -ForegroundColor Green
$loginBody = @{
    email = "test@test.com"
    password = "123456"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginBody

Write-Host "Kết quả đăng nhập:" -ForegroundColor Yellow
$loginResponse | ConvertTo-Json

# Lưu userId để test farm
$userId = $loginResponse.userId
Write-Host "`n=== TEST LẤY FARM (User ID: $userId) ===" -ForegroundColor Green
$farmResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/farm/$userId" `
    -Method GET

Write-Host "Thông tin farm:" -ForegroundColor Yellow
$farmResponse | ConvertTo-Json

