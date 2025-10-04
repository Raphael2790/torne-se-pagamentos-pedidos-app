#!/bin/bash

# Local Build Script for TorneSe.PagamentosPedidos.App
# This script only builds and packages without deploying to AWS

set -e

# Configuration
PROJECT_NAME="TorneSe.PagamentosPedidos.App"
PROJECT_PATH="src/TorneSe.PagamentosPedidos.App"
PUBLISH_DIR="publish"
ZIP_FILE="${PROJECT_NAME}.zip"

echo "=========================================="
echo "Local Build for ${PROJECT_NAME}"
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

# Create ZIP package
if command -v zip &> /dev/null; then
    echo "📦 Creating ZIP package..."
    zip -r "../${ZIP_FILE}" . -x "*.pdb" "*.xml"
else
    echo "⚠️  ZIP not available, creating tar.gz package..."
    cd ..
    tar -czf "${PROJECT_NAME}.tar.gz" -C "${PUBLISH_DIR}" .
    ZIP_FILE="${PROJECT_NAME}.tar.gz"
    cd "${PUBLISH_DIR}"
fi

# Go back to root directory
cd ..

# Verify package was created
if [ -f "${ZIP_FILE}" ]; then
    echo "✅ Package created successfully: ${ZIP_FILE}"
    echo "📊 Package size: $(du -h "${ZIP_FILE}" | cut -f1)"
else
    echo "❌ Failed to create deployment package"
    exit 1
fi

echo ""
echo "=========================================="
echo "🎉 Local build completed successfully!"
echo "=========================================="
echo "📦 Package: ${ZIP_FILE}"
echo "📊 Package size: $(du -h "${ZIP_FILE}" | cut -f1)"
echo ""
echo "🚀 To deploy to AWS, run:"
echo "   ./deploy/build-and-deploy.sh"
echo "=========================================="