# Regenerates the sample catalog used by the storefront's demo mode from a running API.
#
#   1. Start the API (see the README).
#   2. Run:  pwsh tools/generate-demo-data.ps1 -BaseUrl http://localhost:5086
#
# The output is committed at LinkDev.Talabat.APIs/wwwroot/store/demo-data.json.
param(
    [string]$BaseUrl = "http://localhost:5086",
    [string]$Output = (Join-Path $PSScriptRoot "..\LinkDev.Talabat.APIs\wwwroot\store\demo-data.json")
)

$ErrorActionPreference = "Stop"
$api = "$BaseUrl/api"

# Product photos are served by the API itself; keep only the path so it works on any host.
function Get-Path($url) { if ($url) { ([Uri]$url).AbsolutePath } else { $null } }

$products = @()
$page = 1
do {
    $result = Invoke-RestMethod "$api/products?pageIndex=$page&pageSize=10"
    $products += $result.data | ForEach-Object {
        [ordered]@{
            id = $_.id; name = $_.name; description = $_.description; pictureUrl = Get-Path $_.pictureUrl
            price = $_.price; category = $_.category; brand = $_.brand; brandId = $_.brandId; categoryId = $_.categoryId
        }
    }
    $page++
} while ($products.Count -lt $result.count)

$brands = @(Invoke-RestMethod "$api/products/brands" | ForEach-Object { $_ })
$categories = @(Invoke-RestMethod "$api/products/categories" | ForEach-Object { $_ })

# Delivery methods need a signed-in user, so use a throw-away account.
$suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
$user = Invoke-RestMethod -Method Post "$api/account/register" -ContentType "application/json" -Body (@{
    displayName = "Seed"; userName = "seed$suffix"; email = "seed$suffix@test.com"; phone = "01000000000"; password = "P@ssw0rd!"
} | ConvertTo-Json)
$deliveryMethods = @(Invoke-RestMethod "$api/orders/deliveryMethods" -Headers @{ Authorization = "Bearer $($user.token)" } | ForEach-Object { $_ })

$data = [ordered]@{ products = $products; brands = $brands; categories = $categories; deliveryMethods = $deliveryMethods }
$json = $data | ConvertTo-Json -Depth 6

# UTF-8 without a byte-order mark, so the browser parses it as-is.
[IO.File]::WriteAllText((Resolve-Path -LiteralPath (Split-Path $Output)).Path + "\" + (Split-Path $Output -Leaf), $json, (New-Object Text.UTF8Encoding($false)))
Write-Host "Wrote $($products.Count) products, $($brands.Count) brands, $($categories.Count) categories, $($deliveryMethods.Count) delivery methods."
