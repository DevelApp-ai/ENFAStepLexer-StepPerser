# DevelApp.StepLexer Documentation

## Overview

DevelApp.StepLexer is a zero-copy, UTF-8 native lexical analyzer designed for high-performance pattern parsing with advanced PCRE2 support. It implements a forward-only parsing architecture with ambiguity resolution capabilities, making it suitable for both regex pattern parsing and source code tokenization.

## Key Features

### 🚀 Zero-Copy Architecture
- **ZeroCopyStringView**: Efficient string slicing without memory allocation
- **UTF-8 Native**: Direct UTF-8 processing without intermediate string conversions
- **Memory Efficient**: Cap'n Proto inspired design patterns for minimal memory footprint

### 🔍 Advanced Pattern Recognition
- **PCRE2 Compatibility**: Comprehensive support for modern regex features
- **Ambiguity Resolution**: Splittable tokens for handling parsing ambiguities
- **Forward-Only Parsing**: No backtracking for predictable performance

### 🏗️ Type-Safe Token System
- **Enumerated Token Types**: Type-safe token classification
- **Position Tracking**: Accurate source location tracking
- **Unicode Support**: Full Unicode code point and property support

## Core Components

### StepLexer Class

The main lexical analyzer that processes input text and generates tokens.

```csharp
public class StepLexer
{
    // Core tokenization methods
    public (SplittableToken token, int newPosition) TokenizeNext(
        ZeroCopyStringView input, 
        int position = 0);
    
    // Pattern-specific tokenization
    public List<SplittableToken> TokenizeRegexPattern(
        ZeroCopyStringView pattern);
}
```

**Key Methods:**
- `TokenizeNext()`: Processes the next token from input
- `TokenizeRegexPattern()`: Specialized regex pattern tokenization
- `Reset()`: Resets lexer state for new input

### SplittableToken Class

Represents tokens that can be split into multiple alternatives during ambiguity resolution.

```csharp
public class SplittableToken
{
    public ZeroCopyStringView Text { get; }
    public TokenType Type { get; set; }
    public int Position { get; }
    public List<SplittableToken>? Alternatives { get; set; }
    
    // Split token into alternatives
    public void Split(params (ZeroCopyStringView text, TokenType type)[] alternatives);
}
```

**Properties:**
- `Text`: Zero-copy view of token content
- `Type`: Enumerated token classification
- `Position`: Character position in source
- `Alternatives`: List of alternative interpretations for ambiguous tokens

### ZeroCopyStringView Struct

Zero-allocation string view for efficient text processing.

```csharp
public readonly struct ZeroCopyStringView : IEquatable<ZeroCopyStringView>
{
    public int Length { get; }
    public bool IsEmpty { get; }
    
    // Efficient slicing without allocation
    public ZeroCopyStringView Slice(int start, int length);
    
    // Direct span access
    public ReadOnlySpan<byte> AsSpan();
    
    // UTF-8 to string conversion when needed
    public override string ToString();
}
```

**Key Features:**
- **Zero Allocation**: Slicing creates views, not copies
- **UTF-8 Native**: Direct byte-level processing
- **Equality Comparison**: Content-based equality checking

### TokenType Enumeration

Comprehensive token classification system supporting PCRE2 features.

```csharp
public enum TokenType
{
    // Basic constructs
    Literal,
    AnyChar,
    CharacterClass,
    NegatedCharacterClass,
    
    // Quantifiers
    ZeroOrMore,
    OneOrMore,
    ZeroOrOne,
    ExactCount,
    RangeCount,
    
    // Anchors
    StartAnchor,
    EndAnchor,
    WordBoundary,
    NonWordBoundary,
    StringStart,      // \A
    StringEnd,        // \Z
    AbsoluteEnd,      // \z
    ContinuePosition, // \G
    
    // Groups and assertions
    GroupStart,
    GroupEnd,
    NonCapturingGroup,
    NamedGroup,
    PositiveLookahead,
    NegativeLookahead,
    PositiveLookbehind,
    NegativeLookbehind,
    
    // Character classes and escapes
    WordChar,         // \w
    NonWordChar,      // \W
    Digit,            // \d
    NonDigit,         // \D
    Whitespace,       // \s
    NonWhitespace,    // \S
    
    // Unicode support
    UnicodeCodePoint, // \x{FFFF}
    UnicodeProperty,  // \p{property}
    UnicodeCategory,  // \P{property}
    UnicodeNewline,   // \R
    
    // POSIX character classes
    PosixAlpha,       // [:alpha:]
    PosixDigit,       // [:digit:]
    PosixSpace,       // [:space:]
    PosixLower,       // [:lower:]
    PosixUpper,       // [:upper:]
    
    // Advanced features
    BackReference,
    NamedBackReference,
    Alternation,
    
    // Special tokens
    Error,
    EndOfInput
}
```

### PatternParser Class

High-level parser controller for pattern processing.

```csharp
public class PatternParser
{
    public PatternParser(ParserType parserType);
    
    // Zero-copy pattern parsing
    public bool ParsePattern(ReadOnlySpan<byte> utf8Pattern, string terminalName);
    
    // Access parsed tokens
    public List<SplittableToken> GetTokens();
}
```

**Parser Types:**
- `ParserType.Regex`: Regular expression pattern parsing
- `ParserType.Grammar`: Grammar-based pattern parsing

## Advanced Features

### Ambiguity Resolution

The StepLexer handles parsing ambiguities through token splitting:

```csharp
// Example: Ambiguous quantifier interpretation
var token = new SplittableToken(view, TokenType.Literal, 0);

// Split into alternatives when ambiguity detected
token.Split(
    (view.Slice(0, 1), TokenType.Literal),
    (view.Slice(0, 2), TokenType.RangeCount)
);
```

### Unicode Support

Comprehensive Unicode handling with zero-copy efficiency:

```csharp
// Unicode code points: \x{1F600}
// Unicode properties: \p{L}, \P{N}
// Unicode newlines: \R (any Unicode newline sequence)
```

### Performance Optimization

- **Forward-Only Parsing**: No backtracking for predictable performance
- **Zero-Copy Operations**: Minimal memory allocations
- **UTF-8 Native**: Direct byte processing without encoding conversions
- **Lazy Evaluation**: Tokens processed on-demand

## Usage Examples

### Basic Tokenization

```csharp
using DevelApp.StepLexer;
using System.Text;

// Create lexer
var lexer = new StepLexer();

// Prepare UTF-8 input
var pattern = @"\d{2,4}-\w+@[a-z]+\.com";
var utf8Data = Encoding.UTF8.GetBytes(pattern);
var input = new ZeroCopyStringView(utf8Data);

// Tokenize
var tokens = lexer.TokenizeRegexPattern(input);

foreach (var token in tokens)
{
    Console.WriteLine($"{token.Type}: {token.Text}");
}
```

### Pattern Parser Usage

```csharp
using DevelApp.StepLexer;

// Create pattern parser for regex
var parser = new PatternParser(ParserType.Regex);

// Parse pattern with zero-copy
var pattern = @"[a-zA-Z][a-zA-Z0-9]*";
var utf8Pattern = Encoding.UTF8.GetBytes(pattern);

bool success = parser.ParsePattern(utf8Pattern, "identifier");

if (success)
{
    var tokens = parser.GetTokens();
    // Process tokens...
}
```

### Unicode Pattern Processing

```csharp
// Unicode-aware pattern
var unicodePattern = @"\p{L}+\x{20}\p{N}+";
var utf8Data = Encoding.UTF8.GetBytes(unicodePattern);
var view = new ZeroCopyStringView(utf8Data);

var tokens = lexer.TokenizeRegexPattern(view);
// Handles Unicode properties and code points
```

## Error Handling

The StepLexer provides comprehensive error handling:

```csharp
try
{
    var tokens = lexer.TokenizeRegexPattern(input);
}
catch (ENFA_RegexBuild_Exception ex)
{
    Console.WriteLine($"Regex parsing error: {ex.Message}");
    Console.WriteLine($"Position: {ex.Location}");
}
catch (ENFA_Exception ex)
{
    Console.WriteLine($"General lexer error: {ex.Message}");
}
```

## Design Principles

### 1. Zero-Copy Performance
- All string operations use `ZeroCopyStringView`
- Minimal memory allocations during tokenization
- Direct UTF-8 processing without intermediate conversions

### 2. Forward-Only Architecture
- No backtracking to ensure predictable performance
- Conflicts with complex regex features (atomic grouping, recursion)
- Alternative approaches for complex constructs

### 3. Type Safety
- Enumerated token types prevent classification errors
- Compile-time verification of token handling
- Clear separation between different token categories

### 4. Unicode First
- Native UTF-8 processing throughout
- Full Unicode code point support
- Advanced Unicode property classes

## Limitations

### By Design Exclusions

The StepLexer intentionally excludes certain PCRE2 features that conflict with its forward-only architecture:

1. **Atomic Grouping** (`(?>...)`)
   - Requires backtracking prevention mechanisms
   - Conflicts with forward-only parsing paradigm

2. **Possessive Quantifiers** (`*+`, `++`, `?+`)
   - Similar to atomic grouping requirements
   - Would compromise zero-copy performance

3. **Recursive Patterns** (`(?R)`, `(?&name)`)
   - Adds complexity to lexer architecture
   - Better handled by grammar-based StepParser

These limitations are architectural decisions that maintain the lexer's performance and simplicity advantages.

## Integration with StepParser

The StepLexer integrates seamlessly with DevelApp.StepParser for complete parsing solutions:

```csharp
// Lexer tokenizes input
var tokens = stepLexer.TokenizeRegexPattern(pattern);

// Parser builds parse trees and semantic graphs
var parseResult = stepParser.Parse(tokens);
var cognitiveGraph = parseResult.CognitiveGraph;
```

## Testing

Comprehensive test coverage includes:

- **Zero-copy operations**: Memory allocation verification
- **Unicode handling**: Multi-byte character processing
- **Error conditions**: Invalid pattern handling
- **Performance benchmarks**: Throughput and memory usage
- **PCRE2 compliance**: Feature compatibility testing

## Best Practices

1. **Use UTF-8 Input**: Avoid string-to-byte conversions when possible
2. **Reuse Lexer Instances**: Initialize once, tokenize multiple patterns
3. **Handle Alternatives**: Check for `SplittableToken.Alternatives` in ambiguous cases
4. **Error Recovery**: Implement robust error handling for invalid patterns
5. **Performance Monitoring**: Profile memory usage for large-scale processing

## See Also

- [DevelApp.StepParser Documentation](StepParser.md)
- [PCRE2 Support Matrix](PCRE2-Support.md)
- [Grammar File Creation Guide](Grammar_File_Creation_Guide.md)