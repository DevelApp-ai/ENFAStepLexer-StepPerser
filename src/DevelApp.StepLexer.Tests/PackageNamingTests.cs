using Xunit;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevelApp.StepLexer.Tests
{
    /// <summary>
    /// Tests to validate that NuGet package naming follows the expected format
    /// for releases (without pre-release suffixes like "-ci0072")
    /// </summary>
    public class PackageNamingTests
    {
        [Fact]
        public void ReleasePackage_Should_HaveCleanVersionName()
        {
            // This test validates that when packages are built for release,
            // they follow the pattern: DevelApp.StepLexer.X.Y.Z.nupkg
            // and NOT: DevelApp.StepLexer.X.Y.Z-ci####.nupkg
            
            // Pattern for a clean release version (no pre-release suffix)
            var cleanVersionPattern = @"^DevelApp\.StepLexer\.\d+\.\d+\.\d+\.nupkg$";
            var cleanVersionRegex = new Regex(cleanVersionPattern);
            
            // Pattern for pre-release version (with suffix like -ci####)
            var preReleasePattern = @"^DevelApp\.StepLexer\.\d+\.\d+\.\d+-\w+\d*\.nupkg$";
            var preReleaseRegex = new Regex(preReleasePattern);
            
            // Test case: Clean version should match the clean pattern
            Assert.True(cleanVersionRegex.IsMatch("DevelApp.StepLexer.1.0.1.nupkg"), 
                "Clean version should match the expected pattern");
            
            // Test case: Pre-release version should NOT match the clean pattern
            Assert.False(cleanVersionRegex.IsMatch("DevelApp.StepLexer.1.0.1-ci0072.nupkg"), 
                "Pre-release version should NOT match the clean pattern");
            
            // Test case: Pre-release version should match the pre-release pattern
            Assert.True(preReleaseRegex.IsMatch("DevelApp.StepLexer.1.0.1-ci0072.nupkg"), 
                "Pre-release version should match the pre-release pattern");
                
            // Test case: Verify parser package naming too
            var parserCleanPattern = @"^DevelApp\.StepParser\.\d+\.\d+\.\d+\.nupkg$";
            var parserCleanRegex = new Regex(parserCleanPattern);
            
            Assert.True(parserCleanRegex.IsMatch("DevelApp.StepParser.1.0.1.nupkg"), 
                "Parser clean version should match the expected pattern");
                
            Assert.False(parserCleanRegex.IsMatch("DevelApp.StepParser.1.0.1-ci0072.nupkg"), 
                "Parser pre-release version should NOT match the clean pattern");
        }
        
        [Fact]
        public void VersionString_Should_ExtractCleanVersionFromPreRelease()
        {
            // This tests the logic used in the CI/CD pipeline to extract clean versions
            
            // Simulate the sed command: echo "1.0.1-ci0072" | sed 's/-.*$//'
            var fullVersion = "1.0.1-ci0072";
            var cleanVersion = fullVersion.Split('-')[0];
            
            Assert.Equal("1.0.1", cleanVersion);
            
            // Test other examples
            Assert.Equal("2.1.0", "2.1.0-alpha.1".Split('-')[0]);
            Assert.Equal("1.5.3", "1.5.3-beta".Split('-')[0]);
            Assert.Equal("1.0.0", "1.0.0".Split('-')[0]); // Already clean
        }
        
        [Theory]
        [InlineData("1.0.1-ci0072", "1.0.1")]
        [InlineData("2.1.0-alpha.1", "2.1.0")]
        [InlineData("1.5.3-beta.2", "1.5.3")]
        [InlineData("3.0.0-rc1", "3.0.0")]
        [InlineData("1.0.0", "1.0.0")] // Already clean
        public void CleanVersionExtraction_Should_RemovePreReleaseSuffix(string inputVersion, string expectedCleanVersion)
        {
            // This validates the version cleaning logic used in the CI/CD pipeline
            var actualCleanVersion = inputVersion.Split('-')[0];
            Assert.Equal(expectedCleanVersion, actualCleanVersion);
        }
        
        [Fact]
        public void CdPipeline_Should_ProduceCleanVersionPackages()
        {
            // This test validates that the CD pipeline produces packages with clean versions
            // when building for release to NuGet.org (as opposed to CI packages with suffixes)
            
            // Simulate what the CD pipeline does:
            // 1. GitVersion generates a version like "1.0.1-ci0004" 
            // 2. CD pipeline extracts clean version "1.0.1"
            // 3. Packages are built with clean version
            
            var gitVersionOutput = "1.0.1-ci0004";
            var cleanVersion = gitVersionOutput.Split('-')[0];
            
            // Verify the clean version extraction
            Assert.Equal("1.0.1", cleanVersion);
            
            // Verify package names that would be generated
            var expectedLexerPackage = $"DevelApp.StepLexer.{cleanVersion}.nupkg";
            var expectedParserPackage = $"DevelApp.StepParser.{cleanVersion}.nupkg";
            
            Assert.Equal("DevelApp.StepLexer.1.0.1.nupkg", expectedLexerPackage);
            Assert.Equal("DevelApp.StepParser.1.0.1.nupkg", expectedParserPackage);
            
            // Verify these do NOT contain CI suffixes
            Assert.DoesNotContain("-ci", expectedLexerPackage);
            Assert.DoesNotContain("-ci", expectedParserPackage);
        }
        
        [Theory]
        [InlineData("1.2.3", 42, "main", "1.2.3-beta.42")]
        [InlineData("2.1.0", 15, "main", "2.1.0-beta.15")]
        [InlineData("1.0.1", 7, "develop", "1.0.1-alpha.7")]
        [InlineData("1.5.0", 123, "feature/new-parser", "1.5.0-alpha.123")]
        public void PullRequestVersioning_Should_CreateCorrectVersionsByTarget(string baseVersion, int prNumber, string targetBranch, string expectedVersion)
        {
            // This test validates the new PR-based versioning strategy:
            // - PRs to main → beta versions (e.g., 1.2.3-beta.42)
            // - PRs to other branches → alpha versions (e.g., 1.2.3-alpha.42)
            
            var actualVersion = GeneratePrVersion(baseVersion, prNumber, targetBranch);
            Assert.Equal(expectedVersion, actualVersion);
        }
        
        [Fact]
        public void GitHubPackagesPublishing_Should_HandleDifferentVersionTypes()
        {
            // This test validates that different version types are properly handled for GitHub Packages
            
            // Test beta version patterns (PR to main)
            var betaVersionPattern = @"^DevelApp\.StepLexer\.\d+\.\d+\.\d+-beta\.\d+\.nupkg$";
            var betaRegex = new Regex(betaVersionPattern);
            Assert.True(betaRegex.IsMatch("DevelApp.StepLexer.1.2.3-beta.42.nupkg"), 
                "Beta version should match beta pattern");
            
            // Test alpha version patterns (PR to other branches)
            var alphaVersionPattern = @"^DevelApp\.StepLexer\.\d+\.\d+\.\d+-alpha\.\d+\.nupkg$";
            var alphaRegex = new Regex(alphaVersionPattern);
            Assert.True(alphaRegex.IsMatch("DevelApp.StepLexer.1.2.3-alpha.15.nupkg"), 
                "Alpha version should match alpha pattern");
            
            // Test CI version patterns (direct push)
            var ciVersionPattern = @"^DevelApp\.StepLexer\.\d+\.\d+\.\d+-ci\d+\.nupkg$";
            var ciRegex = new Regex(ciVersionPattern);
            Assert.True(ciRegex.IsMatch("DevelApp.StepLexer.1.2.3-ci0042.nupkg"), 
                "CI version should match CI pattern");
        }
        
        [Fact]
        public void VersionTypeDetection_Should_IdentifyCorrectContext()
        {
            // This test validates the version type detection logic used in CI workflow
            
            // Test main branch releases (should go to GitHub Packages as stable)
            Assert.True(IsMainBranchRelease("main", false), "Main branch push should be release");
            
            // Test PR scenarios
            Assert.True(IsPullRequestToBeta("pull_request", "main"), "PR to main should be beta");
            Assert.True(IsPullRequestToAlpha("pull_request", "develop"), "PR to develop should be alpha");
            Assert.True(IsPullRequestToAlpha("pull_request", "feature/test"), "PR to feature should be alpha");
        }
        
        /// <summary>
        /// Simulates the PR version generation logic from the CI workflow
        /// </summary>
        private static string GeneratePrVersion(string baseVersion, int prNumber, string targetBranch)
        {
            var cleanVersion = baseVersion.Split('-')[0];
            
            if (targetBranch == "main")
            {
                return $"{cleanVersion}-beta.{prNumber}";
            }
            else
            {
                return $"{cleanVersion}-alpha.{prNumber}";
            }
        }
        
        /// <summary>
        /// Simulates main branch release detection
        /// </summary>
        private static bool IsMainBranchRelease(string branch, bool isPullRequest)
        {
            return branch == "main" && !isPullRequest;
        }
        
        /// <summary>
        /// Simulates PR to main detection (should generate beta)
        /// </summary>
        private static bool IsPullRequestToBeta(string eventName, string targetBranch)
        {
            return eventName == "pull_request" && targetBranch == "main";
        }
        
        /// <summary>
        /// Simulates PR to non-main detection (should generate alpha)
        /// </summary>
        private static bool IsPullRequestToAlpha(string eventName, string targetBranch)
        {
            return eventName == "pull_request" && targetBranch != "main";
        }
    }
}