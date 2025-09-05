using Xunit;
using FluentAssertions;
using ENFA_Parser.vNext;
using System.Text;

namespace ENFA_Parser.Tests.vNext
{
    public class ZeroCopyStringViewTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesView()
        {
            // Arrange
            var utf8Data = Encoding.UTF8.GetBytes("Hello, World!");
            
            // Act
            var view = new ZeroCopyStringView(utf8Data);
            
            // Assert
            view.Length.Should().Be(utf8Data.Length);
            view.IsEmpty.Should().BeFalse();
        }
        
        [Fact]
        public void Slice_ValidRange_ReturnsCorrectSlice()
        {
            // Arrange
            var utf8Data = Encoding.UTF8.GetBytes("Hello, World!");
            var view = new ZeroCopyStringView(utf8Data);
            
            // Act
            var slice = view.Slice(7, 5); // "World"
            
            // Assert
            slice.Length.Should().Be(5);
            slice.ToString().Should().Be("World");
        }
        
        [Fact]
        public void Indexer_ValidIndex_ReturnsCorrectByte()
        {
            // Arrange
            var utf8Data = Encoding.UTF8.GetBytes("ABC");
            var view = new ZeroCopyStringView(utf8Data);
            
            // Act & Assert
            view[0].Should().Be((byte)'A');
            view[1].Should().Be((byte)'B');
            view[2].Should().Be((byte)'C');
        }
        
        [Fact]
        public void Equals_SameContent_ReturnsTrue()
        {
            // Arrange
            var utf8Data1 = Encoding.UTF8.GetBytes("test");
            var utf8Data2 = Encoding.UTF8.GetBytes("test");
            var view1 = new ZeroCopyStringView(utf8Data1);
            var view2 = new ZeroCopyStringView(utf8Data2);
            
            // Act & Assert
            view1.Equals(view2).Should().BeTrue();
            (view1 == view2).Should().BeTrue();
        }
        
        [Fact]
        public void ToString_ValidUtf8_ReturnsCorrectString()
        {
            // Arrange
            var originalString = "Hello, 世界! 🌍";
            var utf8Data = Encoding.UTF8.GetBytes(originalString);
            var view = new ZeroCopyStringView(utf8Data);
            
            // Act
            var result = view.ToString();
            
            // Assert
            result.Should().Be(originalString);
        }
    }
    
    public class UTF8UtilsTests
    {
        [Theory]
        [InlineData((byte)'A', 'A', true)]
        [InlineData((byte)'Z', 'Z', true)]
        [InlineData((byte)'0', '0', true)]
        [InlineData((byte)'!', '!', true)]
        [InlineData((byte)'A', 'B', false)]
        [InlineData(200, 'A', false)] // Non-ASCII byte
        public void IsAsciiChar_VariousInputs_ReturnsExpected(byte input, char expected, bool expectedResult)
        {
            // Arrange
            var bytes = new byte[] { input };
            
            // Act
            var result = UTF8Utils.IsAsciiChar(bytes, 0, expected);
            
            // Assert
            result.Should().Be(expectedResult);
        }
        
        [Theory]
        [InlineData("A", 0x41, 1)]
        [InlineData("café", 0x63, 1)] // 'c'
        [InlineData("🌍", 0x1F30D, 4)] // Earth emoji
        public void GetNextCodepoint_VariousInputs_ReturnsExpected(string input, uint expectedCodepoint, int expectedBytes)
        {
            // Arrange
            var utf8Bytes = Encoding.UTF8.GetBytes(input);
            
            // Act
            var (codepoint, bytesConsumed) = UTF8Utils.GetNextCodepoint(utf8Bytes, 0);
            
            // Assert
            codepoint.Should().Be(expectedCodepoint);
            bytesConsumed.Should().Be(expectedBytes);
        }
        
        [Theory]
        [InlineData((byte)'0', true)]
        [InlineData((byte)'9', true)]
        [InlineData((byte)'a', false)]
        [InlineData((byte)'A', false)]
        [InlineData((byte)' ', false)]
        public void IsAsciiDigit_VariousInputs_ReturnsExpected(byte input, bool expected)
        {
            // Act
            var result = UTF8Utils.IsAsciiDigit(input);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Theory]
        [InlineData((byte)'a', true)]
        [InlineData((byte)'Z', true)]
        [InlineData((byte)'0', false)]
        [InlineData((byte)' ', false)]
        [InlineData((byte)'!', false)]
        public void IsAsciiLetter_VariousInputs_ReturnsExpected(byte input, bool expected)
        {
            // Act
            var result = UTF8Utils.IsAsciiLetter(input);
            
            // Assert
            result.Should().Be(expected);
        }
    }
    
    public class TwoPhaseParserTests
    {
        [Fact]
        public void Phase1_LexicalScan_SimplePattern_Success()
        {
            // Arrange
            var parser = new TwoPhaseParser();
            var pattern = Encoding.UTF8.GetBytes(@"\d+");
            var view = new ZeroCopyStringView(pattern);
            
            // Act
            var result = parser.Phase1_LexicalScan(view);
            
            // Assert
            result.Should().BeTrue();
            parser.Phase1Results.Should().NotBeEmpty();
        }
        
        [Fact]
        public void Phase1_LexicalScan_ComplexPattern_DetectsAmbiguity()
        {
            // Arrange
            var parser = new TwoPhaseParser();
            var pattern = Encoding.UTF8.GetBytes(@"\x{41}\xFF"); // Ambiguous hex patterns
            var view = new ZeroCopyStringView(pattern);
            
            // Act
            var result = parser.Phase1_LexicalScan(view);
            
            // Assert
            result.Should().BeTrue();
            parser.Phase1Results.Should().ContainSingle(t => t.HasAlternatives);
        }
        
        [Fact]
        public void Phase2_Disambiguation_WithController_Success()
        {
            // Arrange
            var parser = new TwoPhaseParser();
            var controller = new ENFA_Controller(ParserType.Regex);
            var pattern = Encoding.UTF8.GetBytes(@"abc");
            var view = new ZeroCopyStringView(pattern);
            
            // Act
            var phase1Result = parser.Phase1_LexicalScan(view);
            var phase2Result = parser.Phase2_Disambiguation(controller);
            
            // Assert
            phase1Result.Should().BeTrue();
            phase2Result.Should().BeTrue();
        }
    }
    
    public class ENFA_vNext_ControllerTests
    {
        [Fact]
        public void ParsePattern_SimplePattern_Success()
        {
            // Arrange
            var controller = new ENFA_vNext_Controller(ParserType.Regex);
            var pattern = @"\d{2,4}";
            
            // Act
            var result = controller.ParsePattern(pattern, "test_pattern");
            
            // Assert
            result.Should().BeTrue();
            
            var parseResults = controller.GetResults();
            parseResults.Phase1TokenCount.Should().BeGreaterThan(0);
            parseResults.MemoryUsed.Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void ParsePattern_UTF8Bytes_Success()
        {
            // Arrange
            var controller = new ENFA_vNext_Controller(ParserType.Regex);
            var utf8Pattern = Encoding.UTF8.GetBytes(@"[a-z]+");
            
            // Act
            var result = controller.ParsePattern(utf8Pattern, "test_pattern");
            
            // Assert
            result.Should().BeTrue();
            
            var parseResults = controller.GetResults();
            parseResults.MemoryUsed.Should().Be(utf8Pattern.Length);
        }
        
        [Fact]
        public void GetResults_AfterParsing_ReturnsValidStatistics()
        {
            // Arrange
            var controller = new ENFA_vNext_Controller(ParserType.Regex);
            var pattern = @"\w+@\w+\.\w+"; // Email-like pattern
            
            // Act
            controller.ParsePattern(pattern, "email_pattern");
            var results = controller.GetResults();
            
            // Assert
            results.Phase1TokenCount.Should().BeGreaterThan(0);
            results.MemoryUsed.Should().BeGreaterThan(0);
            results.AmbiguityRatio.Should().BeGreaterOrEqualTo(0);
            results.PatternHierarchy.Should().NotBeNullOrEmpty();
        }
    }
}