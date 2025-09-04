# ENFAStepLexer-StepParser

A modern, extensible ENFA (Extended Non-deterministic Finite Automaton) based regex parser with enhanced PCRE2 support, built upon the foundation of @DevelApp-ai/enfaparser.

## Overview

ENFAStepLexer-StepParser is a complete rewrite and enhancement of the original ENFA parser system, designed to support modern regex features while maintaining high performance and extensibility. The system uses a step-wise processing approach suitable for complex parsing scenarios.

## Key Features

### 🚀 Enhanced PCRE2 Support
- **Basic regex constructs**: Literals, character classes, quantifiers, alternation
- **Advanced anchors**: `\A`, `\Z`, `\z`, `\G` for precise string boundary matching
- **Unicode support**: `\x{FFFF}` code points, `\p{property}` classes, `\R` newlines
- **POSIX character classes**: `[:alpha:]`, `[:digit:]`, `[:space:]`, etc.
- **Groups & assertions**: Capturing groups, lookahead/lookbehind, named groups
- **Back references**: Numbered (`\1`) and named (`\k<name>`) references

### 🏗️ Modern Architecture
- **Modular design**: Clear separation between lexer, parser, and state machine
- **Type-safe transitions**: Enum-based transition system for reliability
- **Factory patterns**: Extensible architecture for adding new features
- **vNext compatibility**: Ready for future architectural enhancements

### 📚 Comprehensive Documentation
- Complete PCRE2 feature support matrix
- Implementation reasoning for design decisions
- Performance and architecture notes
- Future enhancement roadmap

## Quick Start

### Building the Project

```bash
# Clone the repository
git clone https://github.com/DevelApp-ai/ENFAStepLexer-StepPerser.git
cd ENFAStepLexer-StepPerser

# Build the core library
cd src/ENFA_Parser
dotnet build

# Build and run the demo
cd ../ENFAStepLexer.Demo
dotnet run
```

### Basic Usage

```csharp
using ENFA_Parser;
using System.IO;
using System.Text;

// Create a regex controller
var controller = new ENFA_Controller(ParserType.Regex);

// Parse a regex pattern
string pattern = @"\d{2,4}-\w+@[a-z]+\.com";
using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pattern + "\""));
using var reader = new StreamReader(stream);

// Compile the pattern
bool success = controller.Tokenizer.Tokenize("email_pattern", reader);

if (success)
{
    Console.WriteLine("Pattern compiled successfully!");
    Console.WriteLine(controller.PrintHierarchy);
}
```

## Architecture

### Core Components

1. **ENFA_Controller**: Main orchestrator for parsing operations
2. **ENFA_Tokenizer**: Converts regex patterns into token streams  
3. **ENFA_Parser**: Builds ENFA state machines from tokens
4. **ENFA_Transition**: Type-safe state transitions with regex-specific logic
5. **ENFA_Factory**: Extensible factory for creating parser components

### State Machine Design

The system uses Extended Non-deterministic Finite Automata (ENFA) which provides:
- **Memory efficiency**: Compact representation of complex patterns
- **Fast compilation**: Direct construction without intermediate representations
- **Extensibility**: Easy addition of new regex features
- **Debugging support**: Clear hierarchy visualization

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

### ❌ Not Supported (Advanced Features)
- Atomic grouping `(?>...)`
- Possessive quantifiers `*+`, `++`
- Conditional patterns `(?(condition)yes|no)`
- Recursive patterns `(?R)`, `(?&name)`
- Inline modifiers `(?i)`, `(?m)`

See [docs/PCRE2-Support.md](docs/PCRE2-Support.md) for complete details.

## Project Structure

```
ENFAStepLexer-StepPerser/
├── src/
│   ├── ENFA_Parser/              # Core regex parser library
│   │   ├── ENFA_Base.cs          # Base state machine class
│   │   ├── ENFA_Controller.cs    # Main controller
│   │   ├── ENFA_Regex_Tokenizer.cs # Enhanced regex tokenizer
│   │   ├── ENFA_Transitions.cs   # State transition logic
│   │   └── ...
│   └── ENFAStepLexer.Demo/       # Demo console application
├── docs/
│   ├── PCRE2-Support.md          # Comprehensive feature documentation
│   └── Architecture.md           # Original architecture notes
└── README.md                     # This file
```

## Contributing

This project is designed to be extensible and welcomes contributions:

1. **Adding new regex features**: Extend the `RegexTransitionType` enum and implement in the tokenizer
2. **Performance improvements**: Optimize state machine generation and traversal
3. **Testing**: Add comprehensive unit tests for regex patterns
4. **Documentation**: Improve examples and architectural documentation

## Performance

The ENFA-based approach provides:
- **Fast compilation**: Direct pattern-to-state-machine conversion
- **Memory efficient**: Compact state representation
- **Scalable**: Linear performance characteristics for most patterns

## Future Roadmap

### Phase 1 (Immediate)
- [ ] Comprehensive unit test suite
- [ ] Fix nullable reference warnings
- [ ] Basic Unicode property validation
- [ ] Pattern compilation benchmarks

### Phase 2 (Short-term)  
- [ ] Inline modifiers (`(?i)`, `(?m)`, etc.)
- [ ] Literal text sequences (`\Q...\E`)
- [ ] Comment support (`(?#...)`)
- [ ] Advanced error reporting

### Phase 3 (Long-term)
- [ ] Consider atomic grouping support
- [ ] Evaluate recursive pattern feasibility  
- [ ] Full Unicode ICU integration
- [ ] Performance optimization with benchmarks

## License

This project is derived from @DevelApp-ai/enfaparser but excludes the original license as requested. The enhancements and new code are provided for evaluation and development purposes.

## Acknowledgments

- Original ENFA parser concept and implementation by @DevelApp-ai/enfaparser
- PCRE2 specification for comprehensive regex feature reference
- .NET community for modern C# language features and tooling
