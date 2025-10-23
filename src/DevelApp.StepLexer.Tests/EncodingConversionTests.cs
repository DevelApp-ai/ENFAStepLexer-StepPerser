using Xunit;
using DevelApp.StepLexer;
using System.Text;
using System.IO;

namespace DevelApp.StepLexer.Tests
{
    public class EncodingConverterTests
    {
        [Fact]
        public void ConvertToUTF8_UTF8Input_ReturnsSameBytes()
        {
            // Arrange
            var testString = "Hello, World!";
            var utf8Bytes = Encoding.UTF8.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf8Bytes, SourceEncoding.UTF8);

            // Assert
            Assert.Equal(utf8Bytes, result);
        }

        [Fact]
        public void ConvertToUTF8_ASCIIInput_ReturnsSameBytes()
        {
            // Arrange
            var testString = "Simple ASCII text";
            var asciiBytes = Encoding.ASCII.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(asciiBytes, SourceEncoding.ASCII);

            // Assert
            Assert.Equal(asciiBytes, result);
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
            var result = EncodingConverter.ConvertToUTF8(latin1Bytes, SourceEncoding.Latin1);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_Windows1252Input_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Extended chars: àéîöü";
            
            // Windows-1252 might not be available on all platforms
            // Use Latin-1 as a fallback which is similar
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(1252);
            }
            catch (NotSupportedException)
            {
                // Skip test if encoding not available
                return;
            }
            
            var windows1252Bytes = encoding.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(windows1252Bytes, SourceEncoding.Windows1252);

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
            var result = EncodingConverter.ConvertToUTF8(utf16Bytes, SourceEncoding.UTF16LE);

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
            var result = EncodingConverter.ConvertToUTF8(utf16Bytes, SourceEncoding.UTF16BE);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_UTF32LEInput_ConvertsCorrectly()
        {
            // Arrange
            var testString = "UTF32 test: Hello 世界";
            var utf32Bytes = Encoding.UTF32.GetBytes(testString);

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf32Bytes, SourceEncoding.UTF32LE);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_UTF32BEInput_ConvertsCorrectly()
        {
            // Arrange
            var testString = "UTF32BE: 你好";
            var utf32LEBytes = Encoding.UTF32.GetBytes(testString);
            
            // Convert LE to BE
            byte[] utf32BEBytes = new byte[utf32LEBytes.Length];
            for (int i = 0; i < utf32LEBytes.Length; i += 4)
            {
                utf32BEBytes[i] = utf32LEBytes[i + 3];
                utf32BEBytes[i + 1] = utf32LEBytes[i + 2];
                utf32BEBytes[i + 2] = utf32LEBytes[i + 1];
                utf32BEBytes[i + 3] = utf32LEBytes[i];
            }

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf32BEBytes, SourceEncoding.UTF32BE);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
        }

        [Fact]
        public void ConvertToUTF8_EmptyInput_ReturnsEmptyArray()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act
            var result = EncodingConverter.ConvertToUTF8(emptyBytes, SourceEncoding.UTF8);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectEncoding_UTF8BOM_ReturnsUTF8()
        {
            // Arrange
            var bom = new byte[] { 0xEF, 0xBB, 0xBF, 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // UTF-8 BOM + "Hello"

            // Act
            var detected = EncodingConverter.DetectEncoding(bom);

            // Assert
            Assert.Equal(SourceEncoding.UTF8, detected);
        }

        [Fact]
        public void DetectEncoding_UTF16LEBOM_ReturnsUTF16LE()
        {
            // Arrange
            var bom = new byte[] { 0xFF, 0xFE, 0x48, 0x00 }; // UTF-16LE BOM

            // Act
            var detected = EncodingConverter.DetectEncoding(bom);

            // Assert
            Assert.Equal(SourceEncoding.UTF16LE, detected);
        }

        [Fact]
        public void DetectEncoding_UTF16BEBOM_ReturnsUTF16BE()
        {
            // Arrange
            var bom = new byte[] { 0xFE, 0xFF, 0x00, 0x48 }; // UTF-16BE BOM

            // Act
            var detected = EncodingConverter.DetectEncoding(bom);

            // Assert
            Assert.Equal(SourceEncoding.UTF16BE, detected);
        }

        [Fact]
        public void DetectEncoding_UTF32LEBOM_ReturnsUTF32LE()
        {
            // Arrange
            var bom = new byte[] { 0xFF, 0xFE, 0x00, 0x00, 0x48, 0x00, 0x00, 0x00 }; // UTF-32LE BOM

            // Act
            var detected = EncodingConverter.DetectEncoding(bom);

            // Assert
            Assert.Equal(SourceEncoding.UTF32LE, detected);
        }

        [Fact]
        public void DetectEncoding_UTF32BEBOM_ReturnsUTF32BE()
        {
            // Arrange
            var bom = new byte[] { 0x00, 0x00, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0x48 }; // UTF-32BE BOM

            // Act
            var detected = EncodingConverter.DetectEncoding(bom);

            // Assert
            Assert.Equal(SourceEncoding.UTF32BE, detected);
        }

        [Fact]
        public void DetectEncoding_ASCIIText_ReturnsASCII()
        {
            // Arrange
            var asciiBytes = Encoding.ASCII.GetBytes("Simple ASCII text without special chars");

            // Act
            var detected = EncodingConverter.DetectEncoding(asciiBytes);

            // Assert
            Assert.Equal(SourceEncoding.ASCII, detected);
        }

        [Fact]
        public void ConvertToUTF8_AutoDetectUTF8BOM_ConvertsCorrectly()
        {
            // Arrange
            var testString = "Hello, World!";
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var utf8Bytes = Encoding.UTF8.GetBytes(testString);
            var withBom = bom.Concat(utf8Bytes).ToArray();

            // Act
            var result = EncodingConverter.ConvertToUTF8(withBom, SourceEncoding.AutoDetect);

            // Assert
            var resultString = Encoding.UTF8.GetString(result);
            Assert.Contains("Hello", resultString);
        }

        [Fact]
        public void ConvertToUTF8_UTF16WithBOM_RemovesBOM()
        {
            // Arrange
            var testString = "Test";
            var utf16WithBOM = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(testString)).ToArray();

            // Act
            var result = EncodingConverter.ConvertToUTF8(utf16WithBOM, SourceEncoding.UTF16LE);

            // Assert
            Assert.Equal(testString, Encoding.UTF8.GetString(result));
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
            var result = parser.ParsePattern(utf16Bytes, SourceEncoding.UTF16LE, "test_pattern");

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
            var result = parser.ParsePattern(latin1Bytes, SourceEncoding.Latin1, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePattern_WithASCIIEncoding_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"\w+";
            var asciiBytes = Encoding.ASCII.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(asciiBytes, SourceEncoding.ASCII, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePatternFromStream_WithAutoDetect_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"\d{2,4}";
            var utf8Bytes = Encoding.UTF8.GetBytes(pattern);
            using var stream = new MemoryStream(utf8Bytes);

            // Act
            var result = parser.ParsePatternFromStream(stream, SourceEncoding.AutoDetect, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePatternFromStream_WithUTF16LE_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"[a-zA-Z0-9]+";
            var utf16Bytes = Encoding.Unicode.GetBytes(pattern);
            using var stream = new MemoryStream(utf16Bytes);

            // Act
            var result = parser.ParsePatternFromStream(stream, SourceEncoding.UTF16LE, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePattern_WithUTF8Encoding_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"hello|world";
            var utf8Bytes = Encoding.UTF8.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(utf8Bytes, SourceEncoding.UTF8, "test_pattern");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ParsePattern_ComplexPatternWithUTF16BE_ParsesCorrectly()
        {
            // Arrange
            var parser = new PatternParser(ParserType.Regex);
            var pattern = @"(foo|bar)\d+";
            var utf16Bytes = Encoding.BigEndianUnicode.GetBytes(pattern);

            // Act
            var result = parser.ParsePattern(utf16Bytes, SourceEncoding.UTF16BE, "test_pattern");

            // Assert
            Assert.True(result);
        }
    }
}
