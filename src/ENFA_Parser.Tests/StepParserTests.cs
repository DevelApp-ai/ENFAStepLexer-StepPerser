using Xunit;
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
            Assert.True(result.Success); // "Parsing should succeed"
            Assert.True(result.Tokens.Count > 0); // "Should produce tokens"
            Assert.NotNull(result.ParseTree); // "Should produce parse tree"
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
            Assert.True(result.Success); // "Should handle ambiguous parse"
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
            Assert.Equal("method", contextStack.Current());
            Assert.True(contextStack.InScope("class"));
            Assert.True(contextStack.InScope("global"));
            Assert.False(contextStack.InScope("unknown"));
            Assert.Equal(3, contextStack.Depth());

            var path = contextStack.GetPath();
            Assert.Equal(new[] { "global", "class", "method" }, path);
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

            Assert.NotNull(globalX);
            Assert.Equal("int", globalX!.Type);

            Assert.NotNull(functionX);
            Assert.Equal("float", functionX!.Type); // Should find local shadowing global

            Assert.NotNull(functionY);
            Assert.Equal("string", functionY!.Type);
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
            Assert.True(lexer.ActivePaths.Count > 0); // "Should maintain multiple paths"
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
            Assert.Equal(utf8Bytes.Length, view.Length);
            Assert.False(view.IsEmpty);

            // Test slicing (zero-copy operation)
            var slice = view.Slice(0, 5);
            Assert.Equal(5, slice.Length);
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
            Assert.True(result.Success);
            Assert.True(memStats.bytesAllocated > 0);
            Assert.True(memStats.activeObjects > 0);
            
            // Memory usage should be reasonable for the input size
            Assert.True(memStats.bytesAllocated < 10000); // "Memory usage should be efficient"
        }
    }
}