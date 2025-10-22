using Xunit;
using DevelApp.StepLexer;
using System.Text;
using System.Linq;

namespace DevelApp.StepLexer.Tests
{
    /// <summary>
    /// Tests for PCRE2 regex features as specified in TDS Section 3.2
    /// </summary>
    public class PCRE2FeatureTests
    {
        private StepLexer CreateLexer()
        {
            return new StepLexer();
        }

        #region Inline Modifiers Tests

        [Theory]
        [InlineData("(?i)", true)]
        [InlineData("(?m)", true)]
        [InlineData("(?s)", true)]
        [InlineData("(?x)", true)]
        [InlineData("(?im)", true)]
        [InlineData("(?ims)", true)]
        [InlineData("(?)", false)]
        [InlineData("(?z)", false)]
        public void ScanGroup_InlineModifiers_RecognizedCorrectly(string pattern, bool shouldBeValid)
        {
            // Arrange
            var lexer = CreateLexer();
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            if (shouldBeValid)
            {
                Assert.Contains(tokens, t => t.Type == TokenType.InlineModifier);
                var modifierToken = tokens.First(t => t.Type == TokenType.InlineModifier);
                Assert.Equal(pattern, modifierToken.Text.ToString());
            }
            else
            {
                // Invalid patterns should not produce InlineModifier tokens
                Assert.DoesNotContain(tokens, t => t.Type == TokenType.InlineModifier);
            }
        }

        [Fact]
        public void ScanGroup_ComplexInlineModifier_ParsedCorrectly()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = "(?imsx)test";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            var modifierToken = tokens.FirstOrDefault(t => t.Type == TokenType.InlineModifier);
            Assert.NotNull(modifierToken);
            Assert.Equal("(?imsx)", modifierToken.Text.ToString());
        }

        #endregion

        #region Literal Text \Q...\E Tests

        [Fact]
        public void ScanEscapeSequence_LiteralText_ParsedCorrectly()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = @"\Q.*+?\E";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            var literalToken = tokens.FirstOrDefault(t => t.Type == TokenType.LiteralText);
            Assert.NotNull(literalToken);
            Assert.Equal(@"\Q.*+?\E", literalToken.Text.ToString());
        }

        [Fact]
        public void ScanEscapeSequence_LiteralTextWithSpecialChars_TreatedAsLiteral()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = @"\Q[a-z]*+?\E";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            var literalToken = tokens.FirstOrDefault(t => t.Type == TokenType.LiteralText);
            Assert.NotNull(literalToken);
            // All special chars inside \Q...\E should be treated as literal
            Assert.Equal(@"\Q[a-z]*+?\E", literalToken.Text.ToString());
        }

        [Fact]
        public void ScanEscapeSequence_LiteralTextWithoutEnd_FallsBackToEscape()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = @"\Qtest";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            // Without \E, \Q should be treated as regular escape
            var escapeToken = tokens.FirstOrDefault(t => t.Type == TokenType.EscapeSequence);
            Assert.NotNull(escapeToken);
        }

        #endregion

        #region Comment Tests

        [Fact]
        public void ScanGroup_Comment_ParsedCorrectly()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = "(?# this is a comment)";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            var commentToken = tokens.FirstOrDefault(t => t.Type == TokenType.RegexComment);
            Assert.NotNull(commentToken);
            Assert.Equal("(?# this is a comment)", commentToken.Text.ToString());
        }

        [Fact]
        public void ScanGroup_CommentWithParentheses_ParsedCorrectly()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = "(?# comment with (nested) parens)";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            var commentToken = tokens.FirstOrDefault(t => t.Type == TokenType.RegexComment);
            Assert.NotNull(commentToken);
            Assert.Contains("(nested)", commentToken.Text.ToString());
        }

        [Fact]
        public void ScanGroup_MultipleComments_AllParsed()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = "(?# first)test(?# second)";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            var comments = tokens.Where(t => t.Type == TokenType.RegexComment).ToList();
            Assert.Equal(2, comments.Count);
        }

        #endregion

        #region Unicode Property Tests

        [Theory]
        [InlineData(@"\p{L}", true)]
        [InlineData(@"\P{N}", true)]
        [InlineData(@"\p{Lu}", true)]
        [InlineData(@"\p{Ll}", true)]
        [InlineData(@"\p{Latin}", true)]
        [InlineData(@"\p{Emoji}", true)]
        [InlineData(@"\p{InvalidProperty}", false)]
        [InlineData(@"\p{}", false)]
        public void ValidateUnicodeProperty_VariousProperties_ValidatedCorrectly(string pattern, bool shouldBeValid)
        {
            // Arrange
            var lexer = CreateLexer();
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var phase2Result = lexer.Phase2_Disambiguation();
            var tokens = lexer.Phase1Results;

            // Assert
            if (shouldBeValid)
            {
                var unicodeToken = tokens.FirstOrDefault(t => t.Type == TokenType.UnicodeProperty);
                Assert.NotNull(unicodeToken);
                Assert.True(phase2Result); // Valid properties should pass phase 2
            }
            else
            {
                // Invalid properties should fail phase 2 validation
                Assert.False(phase2Result);
            }
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void TokenizeRegexPattern_ComplexPatternWithMultipleFeatures_ParsedCorrectly()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = @"(?i)(?# case insensitive)\p{L}+\Q.*\E";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            Assert.Contains(tokens, t => t.Type == TokenType.InlineModifier);
            Assert.Contains(tokens, t => t.Type == TokenType.RegexComment);
            Assert.Contains(tokens, t => t.Type == TokenType.UnicodeProperty);
            Assert.Contains(tokens, t => t.Type == TokenType.LiteralText);
        }

        [Fact]
        public void TokenizeRegexPattern_AllPCRE2Features_IntegratedCorrectly()
        {
            // Arrange
            var lexer = CreateLexer();
            var pattern = @"(?m)^(?# start)\Q[literal]\E\p{Lu}+(?i:case)$";
            var input = Encoding.UTF8.GetBytes(pattern);
            var view = new ZeroCopyStringView(input);

            // Act
            lexer.Phase1_LexicalScan(view);
            var tokens = lexer.Phase1Results;

            // Assert
            // Should have multiple token types from PCRE2 features
            Assert.True(tokens.Count() >= 5);
            
            // Verify key features are present
            var tokenTypes = tokens.Select(t => t.Type).Distinct().ToList();
            Assert.Contains(TokenType.InlineModifier, tokenTypes);
            Assert.Contains(TokenType.RegexComment, tokenTypes);
            Assert.Contains(TokenType.LiteralText, tokenTypes);
        }

        #endregion
    }
}
