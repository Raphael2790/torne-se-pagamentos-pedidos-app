# Local Build Script for TorneSe.PagamentosPedidos.App
# This script only builds and packages without deploying to AWS
# PowerShell version for Windows

# Configuration
$ProjectName = "TorneSe.PagamentosPedidos.App"
$ProjectPath = "src/TorneSe.PagamentosPedidos.App"
$PublishDir = "publish"
$ZipFile = "$ProjectName.zip"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Local Build for $ProjectName" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Clean previous builds
Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}
if (Test-Path $ZipFile) {
    Remove-Item -Force $ZipFile
}

# Navigate to project directory
Write-Host "📁 Navigating to project directory..." -ForegroundColor Yellow
Set-Location $ProjectPath

# Build and publish the Lambda function
Write-Host "🔨 Building and publishing Lambda function..." -ForegroundColor Yellow
try {
    dotnet publish -c Release -r linux-x64 --self-contained false -o "../../$PublishDir"
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host "❌ Build failed: $_" -ForegroundColor Red
    exit 1
}

# Navigate to publish directory
Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
Set-Location "../../$PublishDir"

# Create ZIP package
Write-Host "📦 Creating ZIP package..." -ForegroundColor Yellow
try {
    $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal

    # Get all files except .pdb and .xml files
    $files = Get-ChildItem -Recurse | Where-Object {
        !$_.PSIsContainer -and
        $_.Extension -notin @('.pdb', '.xml')
    }

    # Create the ZIP file
    $zipPath = Join-Path (Get-Location).Parent.FullName $ZipFile

    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)

    foreach ($file in $files) {
        $relativePath = $file.FullName.Substring((Get-Location).FullName.Length + 1)
        $relativePath = $relativePath.Replace('\', '/')

        $entry = $zip.CreateEntry($relativePath, $compressionLevel)
        $entryStream = $entry.Open()
        $fileStream = [System.IO.File]::OpenRead($file.FullName)
        $fileStream.CopyTo($entryStream)
        $fileStream.Close()
        $entryStream.Close()
    }

    $zip.Dispose()
    Write-Host "✅ ZIP package created successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create ZIP package: $_" -ForegroundColor Red
    exit 1
}

# Go back to root directory
Set-Location ..

# Verify package was created
if (Test-Path $ZipFile) {
    $packageSize = (Get-Item $ZipFile).Length
    $packageSizeMB = [math]::Round($packageSize / 1MB, 2)
    Write-Host "✅ Package created successfully: $ZipFile" -ForegroundColor Green
    Write-Host "📊 Package size: $packageSizeMB MB" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to create deployment package" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🎉 Local build completed successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "📦 Package: $ZipFile" -ForegroundColor White
Write-Host "📊 Package size: $packageSizeMB MB" -ForegroundColor White
Write-Host ""
Write-Host "🚀 To deploy to AWS, run:" -ForegroundColor Yellow
Write-Host "   .\deploy\build-and-deploy.ps1" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Cyan