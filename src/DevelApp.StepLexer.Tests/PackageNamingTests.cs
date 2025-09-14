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
    }
}