using Xunit;
using FluentAssertions;
using System.IO;
using System.Text;

namespace ENFA_Parser.Tests
{
    public class ENFA_ControllerTests
    {
        [Fact]
        public void Constructor_RegexType_CreatesValidController()
        {
            // Act
            var controller = new ENFA_Controller(ParserType.Regex);
            
            // Assert
            controller.Should().NotBeNull();
            controller.ParserType.Should().Be(ParserType.Regex);
            controller.Tokenizer.Should().NotBeNull();
            controller.Parser.Should().NotBeNull();
        }
        
        [Theory]
        [InlineData("abc", true)]
        [InlineData(@"\d+", true)]
        [InlineData(@"[a-z]*", true)]
        [InlineData(@"\w+@\w+\.\w+", true)]
        public void Tokenizer_BasicPatterns_SucceedsOrFails(string pattern, bool expectedSuccess)
        {
            // Arrange
            var controller = new ENFA_Controller(ParserType.Regex);
            
            // Act
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pattern + "\""));
            using var reader = new StreamReader(stream);
            var result = controller.Tokenizer.Tokenize("test_pattern", reader);
            
            // Assert
            result.Should().Be(expectedSuccess);
            if (expectedSuccess)
            {
                controller.PrintHierarchy.Should().NotBeNullOrEmpty();
            }
        }
        
        [Fact]
        public void PrintHierarchy_AfterValidTokenization_ReturnsNonEmptyString()
        {
            // Arrange
            var controller = new ENFA_Controller(ParserType.Regex);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test\""));
            using var reader = new StreamReader(stream);
            
            // Act
            controller.Tokenizer.Tokenize("test_pattern", reader);
            var hierarchy = controller.PrintHierarchy;
            
            // Assert
            hierarchy.Should().NotBeNullOrEmpty();
            hierarchy.Should().Contain("test_pattern");
        }
    }
    
    public class RegexFeatureTests
    {
        [Theory]
        [InlineData(@"\A", "String start anchor")]
        [InlineData(@"\Z", "String end anchor")]
        [InlineData(@"\z", "Absolute end anchor")]
        [InlineData(@"\x{41}", "Unicode code point")]
        [InlineData(@"\cA", "Control character")]
        [InlineData(@"\p{L}", "Unicode property")]
        [InlineData(@"[[:alpha:]]", "POSIX character class")]
        [InlineData(@"\R", "Unicode newline")]
        public void EnhancedPCRE2Features_ShouldTokenizeSuccessfully(string pattern, string description)
        {
            // Arrange
            var controller = new ENFA_Controller(ParserType.Regex);
            
            // Act
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pattern + "\""));
            using var reader = new StreamReader(stream);
            var result = controller.Tokenizer.Tokenize($"pcre2_test_{description.Replace(" ", "_")}", reader);
            
            // Assert
            result.Should().BeTrue($"Pattern '{pattern}' ({description}) should tokenize successfully");
        }
        
        [Theory]
        [InlineData(@"(?:abc)", "Non-capturing group")]
        [InlineData(@"(?<name>\w+)", "Named group")]
        [InlineData(@"(?=test)", "Positive lookahead")]
        [InlineData(@"(?!test)", "Negative lookahead")]
        [InlineData(@"(?<=pre)", "Positive lookbehind")]
        [InlineData(@"(?<!pre)", "Negative lookbehind")]
        public void GroupsAndAssertions_ShouldTokenizeSuccessfully(string pattern, string description)
        {
            // Arrange
            var controller = new ENFA_Controller(ParserType.Regex);
            
            // Act
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pattern + "\""));
            using var reader = new StreamReader(stream);
            var result = controller.Tokenizer.Tokenize($"group_test_{description.Replace(" ", "_")}", reader);
            
            // Assert
            result.Should().BeTrue($"Pattern '{pattern}' ({description}) should tokenize successfully");
        }
    }
}