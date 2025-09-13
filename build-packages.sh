#!/bin/bash

# Build NuGet packages for DevelApp.StepLexer and DevelApp.StepParser
# This script builds Release packages locally for testing and distribution

set -e

echo "Building NuGet packages..."
echo "=========================="

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean --configuration Release

# Build projects in Release mode (this will auto-generate packages)
echo "Building Release configuration (packages will be generated automatically)..."
dotnet build --configuration Release

# List generated packages
echo ""
echo "Generated packages:"
echo "==================="
find . -name "*.nupkg" -exec ls -la {} \; 2>/dev/null || echo "No packages found"

# Optionally pack to a common directory
if [ "$1" = "--output-dir" ] && [ -n "$2" ]; then
    OUTPUT_DIR="$2"
    echo ""
    echo "Copying packages to $OUTPUT_DIR..."
    mkdir -p "$OUTPUT_DIR"
    find . -name "*.nupkg" -exec cp {} "$OUTPUT_DIR/" \; 2>/dev/null || echo "No packages to copy"
    echo "Packages copied to: $OUTPUT_DIR"
    ls -la "$OUTPUT_DIR"/*.nupkg 2>/dev/null || echo "No packages in output directory"
fi

echo ""
echo "Package build completed successfully!"
echo ""
echo "To install locally for testing:"
echo "  dotnet add package DevelApp.StepLexer --source ./src/DevelApp.StepLexer/bin/Release"
echo "  dotnet add package DevelApp.StepParser --source ./src/DevelApp.StepParser/bin/Release"
echo ""
echo "To publish to a local NuGet source:"
echo "  dotnet nuget push ./src/DevelApp.StepLexer/bin/Release/DevelApp.StepLexer.*.nupkg --source <your-source>"
echo "  dotnet nuget push ./src/DevelApp.StepParser/bin/Release/DevelApp.StepParser.*.nupkg --source <your-source>"