# ENFAStepLexer-StepParser

A modern, high-performance lexical analysis and parsing system with comprehensive PCRE2 support and CognitiveGraph integration. The system consists of DevelApp.StepLexer for zero-copy tokenization and DevelApp.StepParser for semantic analysis and grammar-based parsing.

## Overview

ENFAStepLexer-StepParser is a complete parsing solution designed for high-performance pattern recognition and semantic analysis. The system uses a two-phase approach: StepLexer handles zero-copy tokenization with PCRE2 support, while StepParser provides grammar-based parsing with CognitiveGraph integration for semantic analysis and code understanding.

## Key Features

### 🚀 DevelApp.StepLexer - Zero-Copy Tokenization
- **Zero-copy architecture**: Memory-efficient string processing with ZeroCopyStringView
- **UTF-8 native processing**: Direct UTF-8 handling without encoding conversions
- **Forward-only parsing**: Predictable performance without backtracking
- **Comprehensive PCRE2 support**: 70+ regex features including Unicode and POSIX classes
- **Ambiguity resolution**: Splittable tokens for handling parsing ambiguities

### 🧠 DevelApp.StepParser - Semantic Analysis
- **CognitiveGraph integration**: Automatic semantic graph construction during parsing
- **GLR-style parsing**: Handles ambiguous grammars efficiently
- **Context-sensitive grammars**: Hierarchical context management for complex languages
- **Symbol table management**: Scope-aware symbol tracking and resolution
- **Grammar inheritance**: Reusable grammar components and DSL composition

### 🔧 Advanced Pattern Support
- **Basic regex constructs**: Literals, character classes, quantifiers, alternation
- **Extended anchors**: `\A`, `\Z`, `\z`, `\G` for precise boundary matching
- **Unicode support**: `\x{FFFF}` code points, `\p{property}` classes, `\R` newlines
- **POSIX character classes**: `[:alpha:]`, `[:digit:]`, `[:space:]`, etc.
- **Groups & assertions**: Capturing groups, lookahead/lookbehind, named groups
- **Back references**: Numbered (`\1`) and named (`\k<name>`) references

### 🏗️ Modern Architecture
- **Modular design**: Clear separation between lexer, parser, and semantic analysis
- **Type-safe transitions**: Enum-based token classification for reliability
- **Performance optimized**: Zero-copy operations and memory-efficient data structures
- **Extensible framework**: Plugin architecture for custom grammar features

### 📚 Comprehensive Documentation
- Complete component documentation for StepLexer and StepParser
- PCRE2 feature support matrix with exclusion explanations
- Grammar creation guide for DSL development
- CognitiveGraph integration examples
- Performance optimization guidelines

## Quick Start

### Building the Project

```bash
# Clone the repository
git clone https://github.com/DevelApp-ai/ENFAStepLexer-StepPerser.git
cd ENFAStepLexer-StepPerser

# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Run the demo
cd src/ENFAStepLexer.Demo
dotnet run
```

### Basic StepLexer Usage

```csharp
using DevelApp.StepLexer;
using System.Text;

// Create a pattern parser for regex
var parser = new PatternParser(ParserType.Regex);

// Parse a regex pattern with zero-copy
string pattern = @"\d{2,4}-\w+@[a-z]+\.com";
var utf8Pattern = Encoding.UTF8.GetBytes(pattern);

bool success = parser.ParsePattern(utf8Pattern, "email_pattern");

if (success)
{
    Console.WriteLine("Pattern compiled successfully!");
    var tokens = parser.GetTokens();
    foreach (var token in tokens)
    {
        Console.WriteLine($"{token.Type}: {token.Text}");
    }
}
```

### Basic StepParser Usage

```csharp
using DevelApp.StepParser;

// Create parser engine
var engine = new StepParserEngine();

// Load grammar for a simple expression language
var grammar = @"
Grammar: SimpleExpr
TokenSplitter: Space

<NUMBER> ::= /[0-9]+/
<IDENTIFIER> ::= /[a-zA-Z][a-zA-Z0-9]*/
<PLUS> ::= '+'
<MINUS> ::= '-'
<WS> ::= /[ \t\r\n]+/ => { skip }

<expr> ::= <expr> <PLUS> <expr>
        | <expr> <MINUS> <expr>
        | <NUMBER>
        | <IDENTIFIER>
";

engine.LoadGrammarFromContent(grammar);

// Parse source code
var result = engine.Parse("x + 42 - y");

if (result.Success)
{
    Console.WriteLine("Parse successful!");
    var cognitiveGraph = result.CognitiveGraph;
    // Access semantic analysis results
}
```

## Architecture

### Core Components

1. **DevelApp.StepLexer**: Zero-copy lexical analyzer
   - `PatternParser`: High-level pattern processing controller
   - `StepLexer`: Core tokenization engine with PCRE2 support
   - `ZeroCopyStringView`: Memory-efficient string operations
   - `SplittableToken`: Ambiguity-aware token representation

2. **DevelApp.StepParser**: Semantic analysis and grammar parsing
   - `StepParserEngine`: Main parsing controller with CognitiveGraph integration
   - `GrammarDefinition`: Complete grammar specification loader
   - `TokenRule`/`ProductionRule`: Grammar component definitions
   - `IContextStack`: Hierarchical context management
   - `IScopeAwareSymbolTable`: Symbol resolution and scoping

### Processing Pipeline

The system uses a two-phase processing approach:

1. **Lexical Analysis Phase (StepLexer)**:
   - UTF-8 input processing with zero-copy efficiency
   - PCRE2-compatible pattern recognition
   - Ambiguity detection and token splitting
   - Forward-only parsing for predictable performance

2. **Semantic Analysis Phase (StepParser)**:
   - Grammar-based syntax tree construction
   - CognitiveGraph integration for semantic analysis
   - Context-sensitive parsing with scope management
   - Symbol table construction and resolution

### Design Philosophy

- **Zero-Copy Performance**: Minimize memory allocations through efficient data structures
- **Forward-Only Parsing**: Avoid backtracking for predictable performance characteristics
- **Semantic Integration**: Automatic semantic graph construction during parsing
- **Modular Architecture**: Clear separation of concerns between lexical and semantic analysis

## PCRE2 Feature Support

### ✅ Fully Supported (70+ features)
- All basic regex constructs and quantifiers
- Character classes and escape sequences  
- Groups, assertions, and back references
- Extended anchors and boundaries
- Unicode code points and properties (basic)
- POSIX character classes

### ⚠️ Partially Supported
- Unicode properties (parsing only, requires runtime implementation)

### ❌ Not Supported (By Design)

The following features are intentionally excluded due to architectural design decisions:

#### Atomic Grouping (`(?>...)`)
- **Conflicts with forward-only parsing architecture**
- **Would require backtracking mechanisms that violate design principles**
- **Compromises zero-copy, single-pass performance advantages**
- **Alternative**: Use grammar-based parsing in StepParser for complex constructs

#### Recursive Pattern Support (`(?R)`, `(?&name)`)
- **Adds unnecessary complexity to lexer architecture**
- **Better handled by grammar-based StepParser for recursive constructs**
- **Would compromise predictable memory usage and performance**
- **Alternative**: Implement balanced parsing through grammar rules rather than regex recursion

#### Other Advanced Features
- Possessive quantifiers (`*+`, `++`)
- Conditional patterns (`(?(condition)yes|no)`)
- Inline modifiers (`(?i)`, `(?m)`)

See [docs/PCRE2-Support.md](docs/PCRE2-Support.md) for complete feature matrix and detailed explanations.

## Project Structure

```
ENFAStepLexer-StepPerser/
├── src/
│   ├── DevelApp.StepLexer/           # Zero-copy lexical analyzer
│   │   ├── StepLexer.cs              # Core tokenization engine
│   │   ├── PatternParser.cs          # High-level pattern controller
│   │   ├── ZeroCopyStringView.cs     # Memory-efficient string operations
│   │   ├── SplittableToken.cs        # Ambiguity-aware tokens
│   │   └── ...
│   ├── DevelApp.StepParser/          # Grammar-based semantic parser  
│   │   ├── StepParserEngine.cs       # Main parsing controller
│   │   ├── GrammarDefinition.cs      # Grammar specification
│   │   ├── TokenRule.cs              # Lexical analysis rules
│   │   ├── ProductionRule.cs         # Syntax analysis rules
│   │   └── ...
│   ├── DevelApp.StepLexer.Tests/     # StepLexer unit tests
│   ├── DevelApp.StepParser.Tests/    # StepParser unit tests
│   └── ENFAStepLexer.Demo/           # Demo console application
├── docs/
│   ├── StepLexer.md                  # Complete StepLexer documentation
│   ├── StepParser.md                 # Complete StepParser documentation
│   ├── PCRE2-Support.md              # Feature support matrix
│   └── Grammar_File_Creation_Guide.md # DSL development guide
└── README.md                         # This file
```

## Documentation

### Component Documentation
- [**StepLexer Documentation**](docs/StepLexer.md) - Comprehensive guide to zero-copy lexical analysis
- [**StepParser Documentation**](docs/StepParser.md) - Complete semantic parsing and CognitiveGraph integration
- [**PCRE2 Support Matrix**](docs/PCRE2-Support.md) - Feature compatibility and exclusion explanations
- [**Grammar Creation Guide**](docs/Grammar_File_Creation_Guide.md) - DSL development and grammar authoring

### Quick Navigation
- **Getting Started**: See [Quick Start](#quick-start) section above
- **Architecture Overview**: [Architecture](#architecture) section
- **Feature Support**: [PCRE2 Feature Support](#pcre2-feature-support) section
- **Performance**: [Performance](#performance) section

## Contributing

This project welcomes contributions in several areas:

### Core Development
1. **Adding new regex features**: Extend TokenType enum and implement in StepLexer
2. **Grammar features**: Enhance StepParser with new grammar constructs
3. **Performance improvements**: Optimize zero-copy operations and memory usage
4. **CognitiveGraph integration**: Improve semantic analysis capabilities

### Testing and Quality
1. **Comprehensive unit tests**: Expand test coverage for edge cases
2. **Performance benchmarks**: Add throughput and memory usage benchmarks
3. **Grammar validation**: Create test suites for grammar files
4. **Documentation examples**: Improve code examples and tutorials

### Documentation
1. **API documentation**: Enhance inline code documentation
2. **Tutorial content**: Create step-by-step guides for common scenarios
3. **Best practices**: Document performance optimization techniques
4. **Integration guides**: Show integration with other parsing tools

## Performance

The StepLexer-StepParser architecture provides:

### StepLexer Performance
- **Zero-copy operations**: No string allocations during tokenization
- **UTF-8 native processing**: Direct byte-level operations
- **Forward-only parsing**: Linear time complexity for most patterns
- **Memory efficient**: Predictable memory usage patterns

### StepParser Performance  
- **Incremental parsing**: Process changes without full re-parsing
- **CognitiveGraph caching**: Semantic analysis result caching
- **Context-aware optimization**: Optimized parsing for specific contexts
- **Symbol table efficiency**: Fast symbol lookup and resolution

### Benchmarks
- **Compilation speed**: Direct pattern-to-token conversion
- **Memory usage**: Minimal allocations with zero-copy design  
- **Scalability**: Linear performance characteristics for typical patterns
- **Throughput**: High-performance processing for large codebases

## Future Roadmap

### Phase 1 (Immediate)
- [ ] Enhanced test coverage for StepLexer and StepParser
- [ ] Performance benchmarking suite
- [ ] Nullable reference warning fixes
- [ ] Advanced Unicode property validation
- [ ] CognitiveGraph optimization

### Phase 2 (Short-term)  
- [ ] Inline modifiers (`(?i)`, `(?m)`, etc.) in StepLexer
- [ ] Literal text sequences (`\Q...\E`)
- [ ] Comment support (`(?#...)`)
- [ ] Advanced error reporting with detailed diagnostics
- [ ] Grammar inheritance improvements

### Phase 3 (Long-term)
- [ ] Evaluate atomic grouping support within forward-parsing constraints
- [ ] Advanced CognitiveGraph analytics
- [ ] Full Unicode ICU integration
- [ ] Real-time parsing for IDEs and editors
- [ ] Performance optimization with machine learning

### Research Areas
- [ ] GPU-accelerated pattern matching
- [ ] Incremental parsing algorithms
- [ ] Advanced semantic analysis techniques
- [ ] Cross-language grammar compilation

## License

This project is derived from @DevelApp/enfaparser but excludes the original license as requested. The enhancements and new code are provided for evaluation and development purposes.

## Acknowledgments

- Modern C# language features and .NET performance optimizations
- PCRE2 specification for comprehensive regex feature reference
- CognitiveGraph project for semantic analysis integration
- Zero-copy design patterns inspired by Cap'n Proto and similar systems
- Community feedback and contributions to parsing and lexical analysis techniques
