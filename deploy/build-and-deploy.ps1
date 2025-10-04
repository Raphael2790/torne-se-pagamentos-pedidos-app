# Build and Deploy Script for TorneSe.PagamentosPedidos.App Lambda Function
# This script builds, packages and deploys the Lambda function to AWS
# PowerShell version for Windows

param(
    [string]$Region = "us-east-1",
    [string]$BucketName = "torne-se-pagamentos-pedidos-app-cloudformation"
)

# Configuration
$ProjectName = "TorneSe.PagamentosPedidos.App"
$S3Bucket = $BucketName
$AwsRegion = $Region
$ProjectPath = "src/TorneSe.PagamentosPedidos.App"
$PublishDir = "publish"
$ZipFile = "$ProjectName.zip"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Building and Deploying $ProjectName" -ForegroundColor Cyan
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

# Create ZIP package for Lambda deployment
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

# AWS S3 Operations
Write-Host "☁️  Preparing AWS S3 deployment..." -ForegroundColor Yellow

# Check if AWS CLI is installed
try {
    $null = Get-Command aws -ErrorAction Stop
} catch {
    Write-Host "❌ AWS CLI is not installed. Please install AWS CLI first." -ForegroundColor Red
    Write-Host "Download from: https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
}

# Check AWS credentials
try {
    $callerIdentity = aws sts get-caller-identity 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "AWS credentials not configured"
    }
    $identity = $callerIdentity | ConvertFrom-Json
    Write-Host "🔑 AWS Account: $($identity.Account)" -ForegroundColor Green
} catch {
    Write-Host "❌ AWS credentials not configured. Please run 'aws configure' first." -ForegroundColor Red
    exit 1
}

# Create S3 bucket if it doesn't exist
Write-Host "🪣 Creating S3 bucket if it doesn't exist..." -ForegroundColor Yellow
try {
    $bucketExists = aws s3 ls "s3://$S3Bucket" 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "🆕 Creating new S3 bucket: $S3Bucket" -ForegroundColor Yellow
        aws s3 mb "s3://$S3Bucket" --region $AwsRegion
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create S3 bucket"
        }

        # Enable versioning on the bucket
        Write-Host "🔄 Enabling versioning on S3 bucket..." -ForegroundColor Yellow
        aws s3api put-bucket-versioning --bucket $S3Bucket --versioning-configuration Status=Enabled
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️  Warning: Failed to enable versioning on S3 bucket" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✅ S3 bucket already exists: $S3Bucket" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Failed to create or access S3 bucket: $_" -ForegroundColor Red
    exit 1
}

# Upload package to S3
Write-Host "⬆️  Uploading package to S3..." -ForegroundColor Yellow
try {
    aws s3 cp $ZipFile "s3://$S3Bucket/" --region $AwsRegion
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload to S3"
    }
    Write-Host "✅ Package uploaded successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to upload package to S3: $_" -ForegroundColor Red
    exit 1
}

# Get S3 object information
try {
    $s3ObjectInfo = aws s3api head-object --bucket $S3Bucket --key $ZipFile --output json 2>$null | ConvertFrom-Json
    $s3ObjectSize = [math]::Round($s3ObjectInfo.ContentLength / 1MB, 2)
    $s3LastModified = $s3ObjectInfo.LastModified
} catch {
    $s3ObjectSize = "Unknown"
    $s3LastModified = "Unknown"
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "📦 Package: $ZipFile" -ForegroundColor White
Write-Host "🪣 S3 Bucket: s3://$S3Bucket/$ZipFile" -ForegroundColor White
Write-Host "📊 Package Size: $packageSizeMB MB (Local) / $s3ObjectSize MB (S3)" -ForegroundColor White
Write-Host "📅 Last Modified: $s3LastModified" -ForegroundColor White
Write-Host "🌍 Region: $AwsRegion" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Update your CloudFormation template with the S3 location" -ForegroundColor White
Write-Host "   2. Deploy/Update your CloudFormation stack" -ForegroundColor White
Write-Host "   3. Test your Lambda function" -ForegroundColor White
Write-Host ""
Write-Host "💡 S3 Location for CloudFormation:" -ForegroundColor Yellow
Write-Host "   Bucket: $S3Bucket" -ForegroundColor White
Write-Host "   Key: $ZipFile" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Cyan