using Xunit;
using DevelApp.StepLexer;
using System.Text;
using System.IO;

namespace DevelApp.StepLexer.Tests
{
    public class EncodingConverterTests
    {
        [Fact]
        public void ConvertToUTF8_UTF8Input_ReturnsSameContent()
        {
            // Arrange
            var testString = "Hello, World!";
            var utf8Bytes = Encoding.UTF8.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf8Bytes, Encoding.UTF8);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_ASCIIInput_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Simple ASCII text";
            var asciiBytes = Encoding.ASCII.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(asciiBytes, Encoding.ASCII);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_Latin1Input_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Café résumé";
            var latin1 = Encoding.GetEncoding("ISO-8859-1");
            var latin1Bytes = latin1.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(latin1Bytes, latin1);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_Windows1252Input_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Extended chars: àéîöü";
            var windows1252 = Encoding.GetEncoding(1252);
            var windows1252Bytes = windows1252.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(windows1252Bytes, 1252);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_UTF16LEInput_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Unicode: 你好 世界 🌍";
            var utf16Bytes = Encoding.Unicode.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf16Bytes, Encoding.Unicode);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_UTF16BEInput_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Unicode: 你好 世界";
            var utf16Bytes = Encoding.BigEndianUnicode.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf16Bytes, Encoding.BigEndianUnicode);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_UTF32Input_ConvertsCorrectly()
        {
            // Arrange
            var testString = "UTF32 test: Hello 世界";
            var utf32Bytes = Encoding.UTF32.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf32Bytes, Encoding.UTF32);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_ByEncodingName_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Test with encoding name";
            var shiftJIS = Encoding.GetEncoding("shift_jis");
            var shiftJISBytes = shiftJIS.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(shiftJISBytes, "shift_jis");

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_EmptyInput_ReturnsEmptyArray()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act
            var result = EncodingConverter.ConvertToUTF8(emptyBytes, Encoding.UTF8);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectEncodingFromBOM_UTF8BOM_ReturnsUTF8()
        {
            // Arrange
            var bom = new byte[] { 0xEF, 0xBB, 0xBF, 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // UTF-8 BOM + "Hello"

            // Act
            var (encoding, bomLength) = EncodingConverter.DetectEncodingFromBOM(bom);

            // Assert
            Assert.Equal(Encoding.UTF8.CodePage, encoding.CodePage);
            Assert.Equal(3, bomLength);
        }

        [Fact]
        public void DetectEncodingFromBOM_UTF16LEBOM_ReturnsUTF16LE()
        {
            // Arrange
            var bom = new byte[] { 0xFF, 0xFE, 0x48, 0x00 }; // UTF-16LE BOM

            // Act
            var (encoding, bomLength) = EncodingConverter.DetectEncodingFromBOM(bom);

            // Assert
            Assert.Equal(Encoding.Unicode.CodePage, encoding.CodePage);
            Assert.Equal(2, bomLength);
        }

        [Fact]
        public void DetectEncodingFromBOM_UTF16BEBOM_ReturnsUTF16BE()
        {
            // Arrange
            var bom = new byte[] { 0xFE, 0xFF, 0x00, 0x48 }; // UTF-16BE BOM

            // Act
            var (encoding, bomLength) = EncodingConverter.DetectEncodingFromBOM(bom);

            // Assert
            Assert.Equal(Encoding.BigEndianUnicode.CodePage, encoding.CodePage);
            Assert.Equal(2, bomLength);
        }

        [Fact]
        public void DetectEncodingFromBOM_NoBOM_ReturnsUTF8()
        {
            // Arrange
            var noBOM = Encoding.ASCII.GetBytes("No BOM here");

            // Act
            var (encoding, bomLength) = EncodingConverter.DetectEncodingFromBOM(noBOM);

            // Assert
            Assert.Equal(Encoding.UTF8.CodePage, encoding.CodePage);
            Assert.Equal(0, bomLength);
        }

        [Fact]
        public void ConvertToUTF8WithAutoDetect_UTF8BOM_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Hello, World!";
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var utf8Bytes = Encoding.UTF8.GetBytes(testString);
            var withBOM = bom.Concat(utf8Bytes).ToArray();

            // Act
            var result = EncodingConverter.ConvertToUTF8WithAutoDetect(withBOM);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void GetAvailableEncodings_ReturnsEncodings()
        {
            // Act
            var encodings = EncodingConverter.GetAvailableEncodings();

            // Assert
            Assert.NotEmpty(encodings);
            Assert.Contains(encodings, e => e.CodePage == 65001); // UTF-8
        }

        [Fact]
        public void IsEncodingAvailable_ValidEncodingName_ReturnsTrue()
        {
            // Act
            var isAvailable = EncodingConverter.IsEncodingAvailable("UTF-8");

            // Assert
            Assert.True(isAvailable);
        }

        [Fact]
        public void IsEncodingAvailable_InvalidEncodingName_ReturnsFalse()
        {
            // Act
            var isAvailable = EncodingConverter.IsEncodingAvailable("invalid-encoding-xyz");

            // Assert
            Assert.False(isAvailable);
        }

        [Fact]
        public void IsEncodingAvailable_ValidCodePage_ReturnsTrue()
        {
            // Act
            var isAvailable = EncodingConverter.IsEncodingAvailable(65001); // UTF-8

            // Assert
            Assert.True(isAvailable);
        }
    }

    public class PatternParserEncodingTests
    {
        [Fact]
        public void ParsePattern_WithUTF16Encoding_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"\d+";
            var utf16Bytes = Encoding.Unicode.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(utf16Bytes, Encoding.Unicode, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePattern_WithLatin1Encoding_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"[a-z]+";
            var latin1 = Encoding.GetEncoding("ISO-8859-1");
            var latin1Bytes = latin1.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(latin1Bytes, latin1, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePattern_WithEncodingName_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"\w+";
            var ascii = Encoding.ASCII;
            var asciiBytes = ascii.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(asciiBytes, "ASCII", "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePattern_WithCodePage_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"[a-z]+";
            var latin1 = Encoding.GetEncoding(28591); // ISO-8859-1 code page
            var latin1Bytes = latin1.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(latin1Bytes, 28591, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePatternFromStream_WithEncoding_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"[a-zA-Z0-9]+";
            var utf16Bytes = Encoding.Unicode.GetBytes(pattern);
            using var stream = new MemoryStream(utf16Bytes);

            // Act
            var result = parser.ParsePatternFromStream(stream, Encoding.Unicode, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePatternFromStreamWithAutoDetect_WithBOM_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"\d{2,4}";
            var bom = Encoding.UTF8.GetPreamble();
            var utf8Bytes = Encoding.UTF8.GetBytes(pattern);
            var withBOM = bom.Concat(utf8Bytes).ToArray();
            using var stream = new MemoryStream(withBOM);

            // Act
            var result = parser.ParsePatternFromStreamWithAutoDetect(stream, "test_pattern");

            // Assert
            Assert.True(result);
        }
    }
}
