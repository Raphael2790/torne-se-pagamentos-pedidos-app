#!/bin/bash

# Build and Deploy Script for TorneSe.PagamentosPedidos.App Lambda Function
# This script builds, packages and deploys the Lambda function to AWS

set -e  # Exit on any error

# Configuration
PROJECT_NAME="TorneSe.PagamentosPedidos.App"
S3_BUCKET="torne-se-pagamentos-pedidos-app-cloudformation"
AWS_REGION="us-east-1"
PROJECT_PATH="src/TorneSe.PagamentosPedidos.App"
PUBLISH_DIR="publish"
ZIP_FILE="${PROJECT_NAME}.zip"

echo "=========================================="
echo "Building and Deploying ${PROJECT_NAME}"
echo "=========================================="

# Clean previous builds
echo "🧹 Cleaning previous builds..."
if [ -d "${PUBLISH_DIR}" ]; then
    rm -rf "${PUBLISH_DIR}"
fi
if [ -f "${ZIP_FILE}" ]; then
    rm -f "${ZIP_FILE}"
fi

# Navigate to project directory
echo "📁 Navigating to project directory..."
cd "${PROJECT_PATH}"

# Build and publish the Lambda function
echo "🔨 Building and publishing Lambda function..."
dotnet publish -c Release -r linux-x64 --self-contained false -o "../../${PUBLISH_DIR}"

# Navigate to publish directory
echo "📦 Creating deployment package..."
cd "../../${PUBLISH_DIR}"

# Create ZIP package for Lambda deployment
if command -v zip &> /dev/null; then
    # Linux/Mac - use zip
    echo "📦 Creating ZIP package using zip command..."
    zip -r "../${ZIP_FILE}" . -x "*.pdb" "*.xml"
elif command -v 7z &> /dev/null; then
    # Use 7zip if available
    echo "📦 Creating ZIP package using 7zip..."
    cd ..
    7z a -tzip "${ZIP_FILE}" "${PUBLISH_DIR}/*" -mx=9 -y -x!"*.pdb" -x!"*.xml"
    cd "${PUBLISH_DIR}"
else
    # Fallback - create tar.gz
    echo "⚠️  ZIP not available, creating tar.gz package..."
    cd ..
    tar -czf "${PROJECT_NAME}.tar.gz" -C "${PUBLISH_DIR}" .
    echo "⚠️  Created ${PROJECT_NAME}.tar.gz instead of ZIP"
    cd "${PUBLISH_DIR}"
fi

# Go back to root directory
cd ..

# Verify package was created
if [ -f "${ZIP_FILE}" ]; then
    echo "✅ Package created successfully: ${ZIP_FILE}"
    echo "📊 Package size: $(du -h "${ZIP_FILE}" | cut -f1)"
elif [ -f "${PROJECT_NAME}.tar.gz" ]; then
    echo "✅ Package created successfully: ${PROJECT_NAME}.tar.gz"
    echo "📊 Package size: $(du -h "${PROJECT_NAME}.tar.gz" | cut -f1)"
    ZIP_FILE="${PROJECT_NAME}.tar.gz"
else
    echo "❌ Failed to create deployment package"
    exit 1
fi

# AWS S3 Operations
echo "☁️  Preparing AWS S3 deployment..."

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI is not installed. Please install AWS CLI first."
    exit 1
fi

# Check AWS credentials
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' first."
    exit 1
fi

# Create S3 bucket if it doesn't exist
echo "🪣 Creating S3 bucket if it doesn't exist..."
if aws s3 ls "s3://${S3_BUCKET}" 2>&1 | grep -q 'NoSuchBucket'; then
    echo "🆕 Creating new S3 bucket: ${S3_BUCKET}"
    aws s3 mb "s3://${S3_BUCKET}" --region "${AWS_REGION}"

    # Enable versioning on the bucket
    echo "🔄 Enabling versioning on S3 bucket..."
    aws s3api put-bucket-versioning \
        --bucket "${S3_BUCKET}" \
        --versioning-configuration Status=Enabled
else
    echo "✅ S3 bucket already exists: ${S3_BUCKET}"
fi

# Upload package to S3
echo "⬆️  Uploading package to S3..."
aws s3 cp "${ZIP_FILE}" "s3://${S3_BUCKET}/" --region "${AWS_REGION}"

# Get S3 object information
S3_OBJECT_SIZE=$(aws s3api head-object --bucket "${S3_BUCKET}" --key "${ZIP_FILE}" --query ContentLength --output text 2>/dev/null || echo "Unknown")
S3_LAST_MODIFIED=$(aws s3api head-object --bucket "${S3_BUCKET}" --key "${ZIP_FILE}" --query LastModified --output text 2>/dev/null || echo "Unknown")

echo ""
echo "=========================================="
echo "🎉 Deployment completed successfully!"
echo "=========================================="
echo "📦 Package: ${ZIP_FILE}"
echo "🪣 S3 Bucket: s3://${S3_BUCKET}/${ZIP_FILE}"
echo "📊 Package Size: $(du -h "${ZIP_FILE}" | cut -f1) (Local) / ${S3_OBJECT_SIZE} bytes (S3)"
echo "📅 Last Modified: ${S3_LAST_MODIFIED}"
echo "🌍 Region: ${AWS_REGION}"
echo ""
echo "🚀 Next steps:"
echo "   1. Update your CloudFormation template with the S3 location"
echo "   2. Deploy/Update your CloudFormation stack"
echo "   3. Test your Lambda function"
echo ""
echo "💡 S3 Location for CloudFormation:"
echo "   Bucket: ${S3_BUCKET}"
echo "   Key: ${ZIP_FILE}"
echo "=========================================="