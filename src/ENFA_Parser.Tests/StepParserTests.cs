using Xunit;
using FluentAssertions;
using ENFA_Parser.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENFA_Parser.Tests
{
    public class StepParserTests
    {
        private StepParserEngine _engine;

        public StepParserTests()
        {
            _engine = new StepParserEngine();
        }

        [Fact]
        public void StepLexer_Should_TokenizeSimpleExpression()
        {
            // Arrange
            var grammar = @"
Grammar: SimpleExpr
TokenSplitter: Space

<NUMBER> ::= /[0-9]+/
<IDENTIFIER> ::= /[a-zA-Z][a-zA-Z0-9]*/
<PLUS> ::= '+'
<MINUS> ::= '-'
<WS> ::= /[ \t\r\n]+/

<expr> ::= <expr> <PLUS> <expr>
         | <expr> <MINUS> <expr>
         | <NUMBER>
         | <IDENTIFIER>
";

            _engine.LoadGrammarFromContent(grammar);

            // Act
            var result = _engine.Parse("x + 42 - y");

            // Assert
            result.Success.Should().BeTrue("Parsing should succeed");
            result.Tokens.Count.Should().BeGreaterThan(0, "Should produce tokens");
            result.ParseTree.Should().NotBeNull("Should produce parse tree");
        }

        [Fact]
        public void StepParser_Should_HandleAmbiguousExpression()
        {
            // Arrange - grammar that can create ambiguous parses
            var grammar = @"
Grammar: AmbiguousExpr
TokenSplitter: Space

<NUMBER> ::= /[0-9]+/
<PLUS> ::= '+'
<TIMES> ::= '*'
<WS> ::= /[ \t\r\n]+/

<expr> ::= <expr> <PLUS> <expr>
         | <expr> <TIMES> <expr>  
         | <NUMBER>
";

            _engine.LoadGrammarFromContent(grammar);

            // Act - expression that could be parsed as (1 + 2) * 3 or 1 + (2 * 3)
            var result = _engine.Parse("1 + 2 * 3");

            // Assert
            result.Success.Should().BeTrue("Should handle ambiguous parse");
        }

        [Fact]
        public void ContextStack_Should_ManageHierarchicalContexts()
        {
            // Arrange
            var contextStack = new ContextStack();

            // Act
            contextStack.Push("global");
            contextStack.Push("class", "MyClass");
            contextStack.Push("method", "myMethod");

            // Assert
            contextStack.Current().Should().Be("method");
            contextStack.InScope("class").Should().BeTrue();
            contextStack.InScope("global").Should().BeTrue();
            contextStack.InScope("unknown").Should().BeFalse();
            contextStack.Depth().Should().Be(3);

            var path = contextStack.GetPath();
            path.Should().BeEquivalentTo(new[] { "global", "class", "method" });
        }

        [Fact]
        public void SymbolTable_Should_TrackScopedDeclarations()
        {
            // Arrange
            var symbolTable = new ScopeAwareSymbolTable();
            var location = new CodeLocation("test.txt", 1, 1, 1, 5);

            // Act
            symbolTable.Declare("x", "int", "global", location);
            symbolTable.Declare("y", "string", "function.main", location);
            symbolTable.Declare("x", "float", "function.main", location); // Shadow global x

            // Assert
            var globalX = symbolTable.Lookup("x", "global");
            var functionX = symbolTable.Lookup("x", "function.main");
            var functionY = symbolTable.Lookup("y", "function.main");

            globalX.Should().NotBeNull();
            globalX!.Type.Should().Be("int");

            functionX.Should().NotBeNull();
            functionX!.Type.Should().Be("float"); // Should find local shadowing global

            functionY.Should().NotBeNull();
            functionY!.Type.Should().Be("string");
        }

        [Fact]
        public void StepParser_Should_HandleMultipleParsingPaths()
        {
            // Arrange - Create a grammar that forces path splitting
            var lexer = new StepLexer();
            lexer.AddRule(new TokenRule("A", "a", "", 1));
            lexer.AddRule(new TokenRule("AB", "ab", "", 2)); // Higher priority, creates ambiguity

            var input = "ab";
            var inputBytes = Encoding.UTF8.GetBytes(input);

            // Act
            lexer.Initialize(new ReadOnlyMemory<byte>(inputBytes));
            
            var stepCount = 0;
            while (!lexer.ActivePaths.All(p => !p.IsValid || p.Position >= inputBytes.Length) && stepCount < 10)
            {
                var result = lexer.Step();
                stepCount++;
                
                if (result.IsComplete) break;
            }

            // Assert
            lexer.ActivePaths.Count.Should().BeGreaterThan(0, "Should maintain multiple paths");
        }

        [Fact]
        public void UTF8Utils_Should_ProcessZeroCopyStrings()
        {
            // Arrange
            var testString = "Hello, World!";
            var utf8Bytes = Encoding.UTF8.GetBytes(testString);
            var memory = new ReadOnlyMemory<byte>(utf8Bytes);

            // Act
            var view = new ZeroCopyStringView(memory);

            // Assert
            view.Length.Should().Be(utf8Bytes.Length);
            view.IsEmpty.Should().BeFalse();

            // Test slicing (zero-copy operation)
            var slice = view.Slice(0, 5);
            slice.Length.Should().Be(5);
        }

        [Fact]
        public void StepParserEngine_Should_ReportMemoryEfficiency()
        {
            // Arrange
            var grammar = @"
Grammar: MemoryTest
<NUMBER> ::= /[0-9]+/
<PLUS> ::= '+'
<expr> ::= <NUMBER> | <expr> <PLUS> <expr>
";

            _engine.LoadGrammarFromContent(grammar);

            // Act
            var result = _engine.Parse("1 + 2 + 3 + 4 + 5");
            var memStats = _engine.GetMemoryStats();

            // Assert
            result.Success.Should().BeTrue();
            memStats.bytesAllocated.Should().BeGreaterThan(0);
            memStats.activeObjects.Should().BeGreaterThan(0);
            
            // Memory usage should be reasonable for the input size
            memStats.bytesAllocated.Should().BeLessThan(10000, "Memory usage should be efficient");
        }
    }
}