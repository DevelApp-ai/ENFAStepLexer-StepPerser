# Package Building Guide

## Overview

This repository contains two NuGet packages:
- **DevelApp.StepLexer** - Advanced UTF-8 tokenization with multi-path support
- **DevelApp.StepParser** - GLR-style multi-path parsing with CognitiveGraph integration

## Development vs Package Workflow

### Development Mode (Default)
- Use **Debug** configuration for local development
- Projects reference each other directly for fast build/test cycles
- No NuGet packages are generated
- All tests run against direct project references

### Package Mode (Release)
- Use **Release** configuration for package generation
- NuGet packages are automatically generated during build
- Package dependencies are properly configured
- Ready for distribution and consumption

## Building Packages Locally

### Quick Package Build
```bash
# Build packages using the included script
./build-packages.sh

# Or build manually
dotnet build --configuration Release
```

### Manual Package Build
```bash
# Clean previous builds
dotnet clean --configuration Release

# Build Release (auto-generates packages)
dotnet build --configuration Release

# Find generated packages
find . -name "*.nupkg"
```

### Copy Packages to Output Directory
```bash
# Build and copy to specific directory
./build-packages.sh --output-dir ./packages
```

## Testing Packages Locally

### Install from Local Source
```bash
# Add packages from local build output
dotnet add package DevelApp.StepLexer --source ./src/DevelApp.StepLexer/bin/Release
dotnet add package DevelApp.StepParser --source ./src/DevelApp.StepParser/bin/Release
```

### Create Local NuGet Source
```bash
# Create and configure local package source
mkdir -p ~/my-nuget-packages
dotnet nuget add source ~/my-nuget-packages --name "Local"

# Copy packages to local source
cp ./src/DevelApp.StepLexer/bin/Release/*.nupkg ~/my-nuget-packages/
cp ./src/DevelApp.StepParser/bin/Release/*.nupkg ~/my-nuget-packages/

# Install from local source
dotnet add package DevelApp.StepLexer --source "Local"
dotnet add package DevelApp.StepParser --source "Local"
```

## Package Dependencies

### DevelApp.StepLexer
- **ICU4N** (v60.1.0-alpha.438) - Unicode processing

### DevelApp.StepParser
- **DevelApp.StepLexer** (v1.0.0) - Tokenization support
- **DevelApp.CognitiveGraph** (v1.0.0) - Semantic analysis

## CI/CD Integration

The repository uses a split CI/CD pipeline approach to ensure clean release package naming:

### CI Pipeline (.github/workflows/ci.yml)
- Runs on pushes and pull requests to main/develop branches
- Builds and tests code with all configurations (Debug/Release)
- Publishes packages with CI version suffixes (e.g., `1.0.1-ci0004`) to GitHub Packages
- Available at: `https://nuget.pkg.github.com/DevelApp-ai/index.json`

### CD Pipeline (.github/workflows/cd.yml)
- Runs **only** on pushes to main branch (not PRs)
- Calculates clean versions by removing pre-release suffixes
- Generates release packages with clean names (e.g., `DevelApp.StepParser.1.0.1.nupkg`)
- Publishes to NuGet.org with production-ready package names
- Creates GitHub releases with proper versioning

### Version Management
- Uses GitVersion for semantic versioning
- CI pipeline: Uses GitVersion output directly (may include CI suffixes)
- CD pipeline: Extracts clean version using `sed 's/-.*$//'` to remove suffixes
- This ensures release packages have clean names like `1.0.1` instead of `1.0.1-ci0004`

### Pipeline Separation Benefits
- Prevents GitVersion reuse between PR builds and release builds
- Ensures NuGet.org packages have clean, professional version names
- Maintains CI package traceability through GitHub Packages
- Allows independent CI testing without affecting release versioning

## Project Structure

```
src/
├── DevelApp.StepLexer/          # Core lexer package
│   ├── bin/Release/             # Generated packages here
│   └── DevelApp.StepLexer.csproj
├── DevelApp.StepParser/         # Core parser package  
│   ├── bin/Release/             # Generated packages here
│   └── DevelApp.StepParser.csproj
├── DevelApp.StepLexer.Tests/    # Lexer tests
├── DevelApp.StepParser.Tests/   # Parser tests
└── ENFAStepLexer.Demo/          # Demo application
```

## Troubleshooting

### Packages Not Generated
- Ensure you're using Release configuration: `dotnet build --configuration Release`
- Check that `GeneratePackageOnBuild` is enabled for Release builds

### Missing Dependencies
- Run `dotnet restore` before building
- Check that all PackageReference dependencies are available

### Demo Application Issues
- Demo uses project references in both Debug and Release modes
- Run `dotnet run --project src/ENFAStepLexer.Demo` to test functionality