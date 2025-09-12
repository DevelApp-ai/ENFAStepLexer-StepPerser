using Xunit;
using ENFA_Parser.Core;
using System.Text;
using System.Linq;

namespace ENFA_Parser.Tests.Core
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
            Assert.Equal(utf8Data.Length, view.Length);
            Assert.False(view.IsEmpty);
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
            Assert.Equal(5, slice.Length);
            Assert.Equal("World", slice.ToString());
        }
        
        [Fact]
        public void Indexer_ValidIndex_ReturnsCorrectByte()
        {
            // Arrange
            var utf8Data = Encoding.UTF8.GetBytes("ABC");
            var view = new ZeroCopyStringView(utf8Data);
            
            // Act & Assert
            Assert.Equal((byte)'A', view[0]);
            Assert.Equal((byte)'B', view[1]);
            Assert.Equal((byte)'C', view[2]);
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
            Assert.True(view1.Equals(view2));
            Assert.True(view1 == view2);
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
            Assert.Equal(originalString, result);
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
            Assert.Equal(expectedResult, result);
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
            Assert.Equal(expectedCodepoint, codepoint);
            Assert.Equal(expectedBytes, bytesConsumed);
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
            Assert.Equal(expected, result);
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
            Assert.Equal(expected, result);
        }
    }
    
    public class StepLexerTwoPhaseTests
    {
        [Fact]
        public void Phase1_LexicalScan_SimplePattern_Success()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = Encoding.UTF8.GetBytes(@"\d+");
            var view = new ZeroCopyStringView(pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            Assert.NotEmpty(lexer.Phase1Results);
        }
        
        [Fact]
        public void Phase1_LexicalScan_ComplexPattern_DetectsAmbiguity()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = Encoding.UTF8.GetBytes(@"\x{41}\xFF"); // Ambiguous hex patterns
            var view = new ZeroCopyStringView(pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            Assert.True(lexer.Phase1Results.Count >= 1);
            Assert.NotEmpty(lexer.Phase1Results.Where(t => t.HasAlternatives));
        }
        
        [Fact]
        public void Phase2_Disambiguation_Success()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = Encoding.UTF8.GetBytes(@"abc");
            var view = new ZeroCopyStringView(pattern);
            
            // Act
            var phase1Result = lexer.Phase1_LexicalScan(view);
            var phase2Result = lexer.Phase2_Disambiguation();
            
            // Assert
            Assert.True(phase1Result);
            Assert.True(phase2Result);
            Assert.NotEmpty(lexer.Phase2Results);
        }
    }
    
    public class PatternParserTests
    {
        [Fact]
        public void ParsePattern_SimplePattern_Success()
        {
            // Arrange
            var controller = new PatternParser(ParserType.Regex);
            var pattern = @"\d{2,4}";
            
            // Act
            var result = controller.ParsePattern(pattern, "test_pattern");
            
            // Assert
            Assert.True(result);
            
            var parseResults = controller.GetResults();
            Assert.True(parseResults.Phase1TokenCount > 0);
            Assert.True(parseResults.MemoryUsed > 0);
        }
        
        [Fact]
        public void ParsePattern_UTF8Bytes_Success()
        {
            // Arrange
            var controller = new PatternParser(ParserType.Regex);
            var utf8Pattern = Encoding.UTF8.GetBytes(@"[a-z]+");
            
            // Act
            var result = controller.ParsePattern(utf8Pattern, "test_pattern");
            
            // Assert
            Assert.True(result);
            
            var parseResults = controller.GetResults();
            Assert.Equal(utf8Pattern.Length, parseResults.MemoryUsed);
        }
        
        [Fact]
        public void GetResults_AfterParsing_ReturnsValidStatistics()
        {
            // Arrange
            var controller = new PatternParser(ParserType.Regex);
            var pattern = @"\w+@\w+\.\w+"; // Email-like pattern
            
            // Act
            controller.ParsePattern(pattern, "email_pattern");
            var results = controller.GetResults();
            
            // Assert
            Assert.True(results.Phase1TokenCount > 0);
            Assert.True(results.MemoryUsed > 0);
            Assert.True(results.AmbiguityRatio >= 0);
            Assert.NotNull(results.PatternHierarchy);
            Assert.NotEmpty(results.PatternHierarchy);
        }
    }
}