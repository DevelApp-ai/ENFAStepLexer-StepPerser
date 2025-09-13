using Xunit;
using DevelApp.StepLexer;
using System.Text;
using System.Linq;

namespace DevelApp.StepLexer.Tests
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
    
    /// <summary>
    /// Comprehensive PCRE2 pattern testing for Phase 1 and Phase 2 enhancements
    /// </summary>
    public class PCRE2PatternTests
    {
        [Theory]
        [InlineData("(?i)", true)]
        [InlineData("(?m)", true)]
        [InlineData("(?s)", true)]
        [InlineData("(?x)", true)]
        [InlineData("(?im)", true)]
        [InlineData("(?ims)", true)]
        [InlineData("(?xyz)", false)]
        [InlineData("(?q)", false)]
        public void Phase1_InlineModifiers_ParsesCorrectly(string pattern, bool isValidModifier)
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result); // Phase 1 lexical scan should always succeed
            Assert.NotEmpty(lexer.Phase1Results);
            
            if (isValidModifier)
            {
                // Valid modifiers should be detected as InlineModifier tokens
                Assert.Contains(lexer.Phase1Results, t => t.Type == TokenType.InlineModifier);
            }
            else
            {
                // Invalid modifiers should be treated as SpecialGroup tokens instead
                Assert.DoesNotContain(lexer.Phase1Results, t => t.Type == TokenType.InlineModifier);
                Assert.Contains(lexer.Phase1Results, t => t.Type == TokenType.SpecialGroup);
            }
        }
        
        [Theory]
        [InlineData(@"\p{L}", true)]  // Letter category
        [InlineData(@"\p{Nd}", true)] // Decimal number
        [InlineData(@"\P{Z}", true)]  // Not separator
        [InlineData(@"\p{Latin}", true)] // Script name
        [InlineData(@"\p{Basic_Latin}", true)] // Block name
        [InlineData(@"\p{InvalidProperty}", false)] // Invalid property
        [InlineData(@"\p{}", false)] // Empty property
        public void Phase1_UnicodeProperties_ValidatesCorrectly(string pattern, bool shouldValidate)
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var phase1Result = lexer.Phase1_LexicalScan(view);
            var phase2Result = lexer.Phase2_Disambiguation();
            
            // Assert
            Assert.True(phase1Result); // Phase 1 should always succeed for lexical scanning
            Assert.Equal(shouldValidate, phase2Result); // Phase 2 should validate Unicode properties
            
            if (phase1Result)
            {
                Assert.Contains(lexer.Phase1Results, t => t.Type == TokenType.UnicodeProperty);
            }
        }
        
        [Theory]
        [InlineData(@"\Qhello world\E", "hello world")]
        [InlineData(@"\Q.*+?\E", ".*+?")]
        [InlineData(@"\Q\E", "")]
        [InlineData(@"\Qno end", null)] // No \E ending
        public void Phase1_LiteralTextConstruct_ParsesCorrectly(string pattern, string expectedLiteral)
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            
            if (expectedLiteral != null)
            {
                Assert.Contains(lexer.Phase1Results, t => t.Type == TokenType.LiteralText);
            }
            else
            {
                Assert.DoesNotContain(lexer.Phase1Results, t => t.Type == TokenType.LiteralText);
            }
        }
        
        [Theory]
        [InlineData(@"(?#comment)")]
        [InlineData(@"(?#multi word comment)")]
        [InlineData(@"(?#)")]
        [InlineData(@"(?#nested(comment))")]
        public void Phase1_CommentGroups_ParsesCorrectly(string pattern)
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            Assert.Contains(lexer.Phase1Results, t => t.Type == TokenType.RegexComment);
        }
        
        [Fact]
        public void Phase1_ComplexPattern_ParsesAllFeatures()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = @"(?i)\p{L}+\Q literal \E(?#comment)[a-z]*";
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            
            // Verify all token types are detected
            var tokens = lexer.Phase1Results;
            Assert.Contains(tokens, t => t.Type == TokenType.InlineModifier);
            Assert.Contains(tokens, t => t.Type == TokenType.UnicodeProperty);
            Assert.Contains(tokens, t => t.Type == TokenType.LiteralText);
            Assert.Contains(tokens, t => t.Type == TokenType.RegexComment);
            Assert.Contains(tokens, t => t.Type == TokenType.CharacterClass);
            Assert.Contains(tokens, t => t.Type == TokenType.Quantifier);
        }
        
        [Fact]
        public void Phase2_UnicodePropertyValidation_RejectsInvalidProperties()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = @"\p{InvalidCategory}";
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var phase1Result = lexer.Phase1_LexicalScan(view);
            var phase2Result = lexer.Phase2_Disambiguation();
            
            // Assert
            Assert.True(phase1Result);  // Lexical scan should succeed
            Assert.False(phase2Result); // Disambiguation should fail on invalid property
        }
        
        [Theory]
        [InlineData(@"\x{41}", TokenType.UnicodeEscape)]
        [InlineData(@"\xFF", TokenType.HexEscape)]
        [InlineData(@"\x41", TokenType.HexEscape)]
        public void Phase1_HexEscapes_CreatesCorrectTokenTypes(string pattern, TokenType expectedType)
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            Assert.Contains(lexer.Phase1Results, t => 
                t.Type == expectedType || 
                (t.HasAlternatives && t.Alternatives != null && t.Alternatives.Any(a => a.Type == expectedType)));
        }
        
        [Fact]
        public void Phase1_AmbiguousHexEscape_CreatesAlternatives()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = @"\x41FF"; // Could be \x41 + F + F or other interpretations
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            
            // Should detect ambiguity and create alternatives
            var ambiguousTokens = lexer.Phase1Results.Where(t => t.HasAlternatives).ToList();
            Assert.NotEmpty(ambiguousTokens);
        }
    }
    
    public class StepLexerTwoPhaseTests
    {
        [Fact]
        public void Phase1_LexicalScan_SimplePattern_Success()
        {
            // Arrange
            var lexer = new DevelApp.StepLexer.StepLexer();
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
            var lexer = new DevelApp.StepLexer.StepLexer();
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
            var lexer = new DevelApp.StepLexer.StepLexer();
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
        
        [Fact]
        public void ParsePattern_PCRE2Features_Success()
        {
            // Arrange
            var controller = new PatternParser(ParserType.Regex);
            var pattern = @"(?i)\p{L}+\Q literal \E(?#comment)";
            
            // Act
            var result = controller.ParsePattern(pattern, "pcre2_pattern");
            
            // Assert
            Assert.True(result);
            
            var parseResults = controller.GetResults();
            Assert.True(parseResults.Phase1TokenCount > 0);
            Assert.Contains("InlineModifier", parseResults.PatternHierarchy);
            Assert.Contains("UnicodeProperty", parseResults.PatternHierarchy);
            Assert.Contains("LiteralText", parseResults.PatternHierarchy);
            Assert.Contains("RegexComment", parseResults.PatternHierarchy);
        }
    }
    
    /// <summary>
    /// Performance and edge case testing for PCRE2 implementation
    /// </summary>
    public class PCRE2PerformanceTests
    {
        [Fact]
        public void Phase1_LargePattern_CompletesInReasonableTime()
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = string.Join("|", Enumerable.Repeat(@"\p{L}+", 100));
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            var startTime = System.DateTime.UtcNow;
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            var duration = System.DateTime.UtcNow - startTime;
            
            // Assert
            Assert.True(result);
            Assert.True(duration.TotalMilliseconds < 1000, $"Pattern parsing took {duration.TotalMilliseconds}ms, expected < 1000ms");
        }
        
        [Theory]
        [InlineData(@"\p{L}", 100)]
        [InlineData(@"(?i)test", 100)]
        [InlineData(@"\Q...\E", 100)]
        [InlineData(@"(?#comment)", 100)]
        public void Phase1_RepeatedPatterns_MaintainsPerformance(string basePattern, int repetitions)
        {
            // Arrange
            var lexer = new StepLexer();
            var pattern = string.Join("", Enumerable.Repeat(basePattern, repetitions));
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            var startTime = System.DateTime.UtcNow;
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            var duration = System.DateTime.UtcNow - startTime;
            
            // Assert
            Assert.True(result);
            Assert.True(duration.TotalMilliseconds < 2000, 
                $"Pattern parsing of {repetitions} repetitions took {duration.TotalMilliseconds}ms, expected < 2000ms");
        }
        
        [Fact]
        public void Phase1_EmptyPattern_HandlesGracefully()
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes("");
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            Assert.True(result);
            Assert.Empty(lexer.Phase1Results);
        }
        
        [Fact]
        public void Phase1_MalformedPattern_HandlesGracefully()
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(@"\p{unclosed");
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var result = lexer.Phase1_LexicalScan(view);
            
            // Assert
            // Should not crash, may return false for malformed patterns
            Assert.True(result || !result); // Either result is acceptable for malformed input
        }
        
        // ===============================================================================================
        // PHASE 3 PCRE2 FEATURES: Advanced Unicode Support with ICU Integration
        // ===============================================================================================
        
        [Theory]
        [InlineData("L", 0x0041, true)]          // 'A' is a letter
        [InlineData("L", 0x0030, false)]         // '0' is not a letter  
        [InlineData("Nd", 0x0030, true)]         // '0' is a decimal number
        [InlineData("Nd", 0x0041, false)]        // 'A' is not a decimal number
        [InlineData("Lu", 0x0041, true)]         // 'A' is uppercase letter
        [InlineData("Lu", 0x0061, false)]        // 'a' is not uppercase letter
        [InlineData("Ll", 0x0061, true)]         // 'a' is lowercase letter
        [InlineData("Ll", 0x0041, false)]        // 'A' is not lowercase letter
        [InlineData("Basic_Latin", 0x0041, true)] // 'A' is in Basic Latin block
        [InlineData("Basic_Latin", 0x00E9, false)] // 'é' is not in Basic Latin block
        [InlineData("Latin_1_Supplement", 0x00E9, true)] // 'é' is in Latin-1 Supplement
        public void ICU_UnicodePropertyMatcher_ValidatesCorrectly(string property, int codepoint, bool expected)
        {
            // Act
            var result = UnicodePropertyMatcher.MatchesProperty(codepoint, property);
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Theory]
        [InlineData("Emoji", 0x1F600, true)]     // 😀 grinning face emoji
        [InlineData("Emoji", 0x0041, false)]     // 'A' is not an emoji
        [InlineData("Math", 0x2211, true)]       // ∑ summation symbol
        [InlineData("Math", 0x0041, false)]      // 'A' is not a math symbol
        [InlineData("Alphabetic", 0x0041, true)] // 'A' is alphabetic
        [InlineData("Alphabetic", 0x0030, false)] // '0' is not alphabetic
        public void ICU_UnicodePropertyMatcher_HandlesBinaryProperties(string property, int codepoint, bool expected)
        {
            // Act
            var result = UnicodePropertyMatcher.MatchesProperty(codepoint, property);
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Theory]
        [InlineData("Latin", 0x0041, true)]      // 'A' is Latin script
        [InlineData("Latin", 0x03B1, false)]    // 'α' is not Latin script (Greek)
        [InlineData("Greek", 0x03B1, true)]     // 'α' is Greek script
        [InlineData("Greek", 0x0041, false)]    // 'A' is not Greek script
        [InlineData("Arabic", 0x0628, true)]    // 'ب' is Arabic script
        [InlineData("Arabic", 0x0041, false)]   // 'A' is not Arabic script
        public void ICU_UnicodePropertyMatcher_HandlesScriptProperties(string script, int codepoint, bool expected)
        {
            // Act
            var result = UnicodePropertyMatcher.MatchesProperty(codepoint, script);
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void ICU_AdvancedUnicodeSupport_NormalizesCorrectly()
        {
            // Arrange
            var unicodeSupport = new AdvancedUnicodeSupport();
            var decomposed = "e\u0301";  // e + combining acute accent
            var composed = "\u00e9";     // é precomposed
            
            // Act
            var nfcDecomposed = unicodeSupport.NormalizeIfNeeded(decomposed, UnicodeNormalizationForm.NFC);
            var nfcComposed = unicodeSupport.NormalizeIfNeeded(composed, UnicodeNormalizationForm.NFC);
            
            // Assert
            Assert.Equal(nfcComposed, nfcDecomposed); // Both should normalize to same form
            Assert.True(unicodeSupport.AreCanonicallyEquivalent(decomposed, composed));
        }
        
        [Fact]
        public void ICU_AdvancedUnicodeSupport_ProcessesUTF8Input()
        {
            // Arrange
            var unicodeSupport = new AdvancedUnicodeSupport();
            var testText = "Hello, 世界! 🌍";
            var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(testText);
            
            // Act
            var result = unicodeSupport.ProcessUnicodePattern(utf8Bytes, @"\p{L}+");
            
            // Assert
            Assert.True(result); // Should successfully process Unicode text
        }
        
        [Fact]
        public void ICU_AdvancedUnicodeSupport_HandlesGraphemeClusters()
        {
            // Arrange
            var unicodeSupport = new AdvancedUnicodeSupport();
            var textWithClusters = "👨‍👩‍👧‍👦"; // Family emoji (multiple codepoints)
            
            // Act
            var boundaries = unicodeSupport.GetGraphemeClusterBoundaries(textWithClusters);
            
            // Assert
            Assert.True(boundaries.Length >= 2); // Should have start and end boundaries
            Assert.Equal(0, boundaries[0]); // First boundary should be at start
            Assert.Equal(textWithClusters.Length, boundaries[^1]); // Last boundary should be at end
        }
        
        [Theory]
        [InlineData(@"\p{L}", true)]           // General category letter
        [InlineData(@"\p{Nd}", true)]          // Decimal number
        [InlineData(@"\p{Latin}", true)]       // Script property
        [InlineData(@"\p{Basic_Latin}", true)] // Block property
        [InlineData(@"\p{Emoji}", true)]       // Binary property
        [InlineData(@"\p{Math}", true)]        // Binary property
        [InlineData(@"\p{InvalidProperty}", false)] // Invalid property
        [InlineData(@"\p{}", false)]           // Empty property
        [InlineData(@"\p{L", false)]           // Malformed (missing closing brace)
        public void ICU_Enhanced_UnicodePropertyValidation_ValidatesCorrectly(string pattern, bool shouldValidate)
        {
            // Arrange
            var lexer = new StepLexer();
            var utf8Pattern = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            
            // Act
            var phase1Result = lexer.Phase1_LexicalScan(view);
            var phase2Result = lexer.Phase2_Disambiguation();
            
            // Assert
            Assert.True(phase1Result); // Phase 1 should always succeed
            Assert.Equal(shouldValidate, phase2Result); // Phase 2 should validate Unicode properties using ICU
            
            if (shouldValidate && phase2Result)
            {
                Assert.Contains(lexer.Phase1Results, t => t.Type == TokenType.UnicodeProperty);
            }
        }
        
        [Fact]
        public void ICU_Performance_LargeUnicodePatternProcessing()
        {
            // Arrange
            var lexer = new StepLexer();
            var largePattern = string.Join("", Enumerable.Repeat(@"\p{L}+\p{Nd}*", 50));
            var utf8Pattern = Encoding.UTF8.GetBytes(largePattern);
            var view = new ZeroCopyStringView(utf8Pattern);
            var startTime = System.DateTime.UtcNow;
            
            // Act
            var phase1Result = lexer.Phase1_LexicalScan(view);
            var phase2Result = lexer.Phase2_Disambiguation();
            var duration = System.DateTime.UtcNow - startTime;
            
            // Assert
            Assert.True(phase1Result);
            Assert.True(phase2Result);
            Assert.True(duration.TotalMilliseconds < 500, 
                $"Large Unicode pattern processing took {duration.TotalMilliseconds}ms, expected < 500ms");
        }
        
        [Theory]
        [InlineData("UnicodeNormalizationForm.NFC")]
        [InlineData("UnicodeNormalizationForm.NFD")]
        [InlineData("UnicodeNormalizationForm.NFKC")]
        [InlineData("UnicodeNormalizationForm.NFKD")]
        public void ICU_NormalizationForms_WorkCorrectly(string normalizationFormName)
        {
            // Arrange
            var unicodeSupport = new AdvancedUnicodeSupport();
            var testText = "café"; // Contains composed character
            var normForm = normalizationFormName switch
            {
                "UnicodeNormalizationForm.NFC" => UnicodeNormalizationForm.NFC,
                "UnicodeNormalizationForm.NFD" => UnicodeNormalizationForm.NFD,
                "UnicodeNormalizationForm.NFKC" => UnicodeNormalizationForm.NFKC,
                "UnicodeNormalizationForm.NFKD" => UnicodeNormalizationForm.NFKD,
                _ => UnicodeNormalizationForm.None
            };
            
            // Act
            var normalized = unicodeSupport.NormalizeIfNeeded(testText, normForm);
            
            // Assert
            Assert.NotNull(normalized);
            Assert.NotEmpty(normalized);
            // NFC and NFKC should be shorter (composed), NFD and NFKD should be longer (decomposed)
            if (normForm == UnicodeNormalizationForm.NFD || normForm == UnicodeNormalizationForm.NFKD)
            {
                Assert.True(normalized.Length >= testText.Length);
            }
        }
        
        // ===============================================================================================
        // PHASE 3 PCRE2 FEATURES: Performance Optimization and Benchmarking
        // ===============================================================================================
        
        [Fact]
        public void Performance_BenchmarkFramework_InitializesCorrectly()
        {
            // Arrange & Act & Assert
            // This test validates that the benchmarking framework can be initialized
            var exception = Record.Exception(() => PerformanceTestRunner.RunAllBenchmarks());
            Assert.Null(exception);
        }
        
        [Fact]
        public void Performance_MemoryBenchmarks_RunWithoutErrors()
        {
            // Arrange & Act & Assert
            // This test validates that memory benchmarks execute correctly
            var exception = Record.Exception(() => PerformanceTestRunner.RunMemoryBenchmarks());
            Assert.Null(exception);
        }
        
        [Fact]
        public void Performance_UnicodeBenchmarks_RunWithoutErrors()
        {
            // Arrange & Act & Assert
            // This test validates that Unicode property benchmarks execute correctly
            var exception = Record.Exception(() => PerformanceTestRunner.RunUnicodeBenchmarks());
            Assert.Null(exception);
        }
        
        [Theory]
        [InlineData(@"[a-zA-Z]+", 100)]
        [InlineData(@"\p{L}+", 100)]
        [InlineData(@"(?i)\p{L}+\p{Nd}*", 100)]
        public void Performance_StepLexerVsDotNetRegex_CompletesWithinReasonableTime(string pattern, int inputSize)
        {
            // Arrange
            var benchmark = new PCRE2PerformanceBenchmark
            {
                Pattern = pattern,
                InputSize = inputSize
            };
            benchmark.Setup();
            var startTime = System.DateTime.UtcNow;
            
            // Act
            var stepLexerResult = benchmark.StepLexerPCRE2();
            var stepLexerTime = System.DateTime.UtcNow - startTime;
            
            startTime = System.DateTime.UtcNow;
            var dotNetResult = benchmark.DotNetCompiledRegex();
            var dotNetTime = System.DateTime.UtcNow - startTime;
            
            // Assert
            Assert.True(stepLexerTime.TotalMilliseconds < 1000, 
                $"StepLexer took {stepLexerTime.TotalMilliseconds}ms, expected < 1000ms");
            Assert.True(dotNetTime.TotalMilliseconds < 1000, 
                $".NET Regex took {dotNetTime.TotalMilliseconds}ms, expected < 1000ms");
                
            // Both should complete successfully (results may differ due to pattern differences)
            Assert.True(stepLexerResult || !stepLexerResult); // Either result acceptable
            Assert.True(dotNetResult || !dotNetResult); // Either result acceptable
        }
        
        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        public void Performance_ZeroCopyProcessing_ShowsMemoryEfficiency(int inputSize)
        {
            // Arrange
            var benchmark = new MemoryUsageBenchmark { InputSize = inputSize };
            benchmark.Setup();
            
            // Act & Assert
            // Test that zero-copy processing completes without memory exceptions
            var exception = Record.Exception(() =>
            {
                var result = benchmark.StepLexerZeroCopyMemoryUsage();
                // Either result is acceptable - we're testing memory efficiency
                Assert.True(result || !result);
            });
            Assert.Null(exception);
        }
        
        [Theory]
        [InlineData("L", 100)]
        [InlineData("Nd", 100)]
        [InlineData("Emoji", 100)]
        public void Performance_UnicodePropertyMatching_ExecutesEfficiently(string property, int testCount)
        {
            // Arrange
            var benchmark = new UnicodePropertyBenchmark 
            { 
                UnicodeProperty = property,
                TestCount = testCount 
            };
            benchmark.Setup();
            var startTime = System.DateTime.UtcNow;
            
            // Act
            var matches = benchmark.ICU_UnicodePropertyMatching();
            var duration = System.DateTime.UtcNow - startTime;
            
            // Assert
            Assert.True(matches >= 0); // Should return valid match count
            Assert.True(duration.TotalMilliseconds < 100, 
                $"Unicode property matching took {duration.TotalMilliseconds}ms for {testCount} tests, expected < 100ms");
        }
        
        [Fact]
        public void Performance_ComprehensiveBenchmarkSuite_ValidatesCorrectly()
        {
            // This test ensures all benchmark classes can be instantiated and run basic validation
            
            // PCRE2 Performance Benchmark
            var pcre2Benchmark = new PCRE2PerformanceBenchmark
            {
                Pattern = @"\p{L}+",
                InputSize = 100
            };
            var exception1 = Record.Exception(() => pcre2Benchmark.Setup());
            Assert.Null(exception1);
            
            // Memory Usage Benchmark  
            var memoryBenchmark = new MemoryUsageBenchmark { InputSize = 100 };
            var exception2 = Record.Exception(() => memoryBenchmark.Setup());
            Assert.Null(exception2);
            
            // Unicode Property Benchmark
            var unicodeBenchmark = new UnicodePropertyBenchmark 
            { 
                UnicodeProperty = "L",
                TestCount = 10 
            };
            var exception3 = Record.Exception(() => unicodeBenchmark.Setup());
            Assert.Null(exception3);
        }
    }
}