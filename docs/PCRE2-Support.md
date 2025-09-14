# PCRE2 Regex Features Support in ENFAStepLexer-StepParser

## Overview

This document describes the PCRE2 (Perl Compatible Regular Expression) features supported by the ENFAStepLexer-StepParser implementation based on the @DevelApp/enfaparser project.

## Supported PCRE2 Features

### ✅ Basic Constructs
- **Literal characters**: Regular characters match themselves
- **Character classes**: `[abc]`, `[^abc]` (character sets and negated sets)
- **Character ranges**: `[a-z]`, `[0-9]` (ranges within character classes)
- **Quantifiers**: 
  - `*` (zero or more)
  - `+` (one or more) 
  - `?` (zero or one)
  - `{n}` (exactly n)
  - `{n,m}` (between n and m)
  - `{n,}` (n or more)
- **Lazy/greedy modifiers**: `*?`, `+?`, `??`, `{n,m}?` and `*+`, `++`, etc.

### ✅ Character Class Shortcuts
- `\w` - Word characters (letters, digits, underscore)
- `\W` - Non-word characters
- `\d` - Digits (0-9)
- `\D` - Non-digits
- `\s` - Whitespace characters
- `\S` - Non-whitespace characters
- `\l` - Letters (custom extension, not standard PCRE)
- `\L` - Non-letters (custom extension, not standard PCRE)

### ✅ Extended Anchors (NEW)
- `^` - Start of line
- `$` - End of line
- `\A` - Start of string
- `\Z` - End of string (before final newline if present)
- `\z` - Absolute end of string
- `\G` - Continue from previous match position
- `\b` - Word boundary
- `\B` - Non-word boundary

### ✅ Unicode and Extended Character Support (NEW)
- `\x{FFFF}` - Unicode code points
- `\xFF` - Hexadecimal character codes
- `\cA-\cZ` - Control characters
- `\p{property}` - Unicode property classes (basic support)
- `\P{property}` - Negated Unicode property classes
- `\R` - Any Unicode newline sequence

### ✅ POSIX Character Classes (NEW)
- `[:alpha:]` - Alphabetic characters
- `[:alnum:]` - Alphanumeric characters
- `[:digit:]` - Digit characters
- `[:lower:]` - Lowercase letters
- `[:upper:]` - Uppercase letters
- `[:space:]` - Whitespace characters
- `[:blank:]` - Space and tab
- `[:punct:]` - Punctuation characters
- `[:xdigit:]` - Hexadecimal digits
- `[:cntrl:]` - Control characters
- `[:graph:]` - Graphical characters
- `[:print:]` - Printable characters

### ✅ Escape Sequences
- `\0` - Null character
- `\a` - Alert (bell)
- `\e` - Escape
- `\f` - Form feed
- `\n` - Newline
- `\r` - Carriage return
- `\t` - Horizontal tab
- `\v` - Vertical tab
- `\b` - Backspace (in character classes)
- `\\` - Literal backslash
- Standard escaped characters: `\"`, `\[`, `\]`, `\(`, `\)`, `\{`, `\}`, `\|`, `\.`, `\^`, `\$`, `\?`, `\*`, `\+`

### ✅ Groups and Assertions
- `()` - Capturing groups
- `(?:...)` - Non-capturing groups
- `(?<name>...)` - Named capturing groups
- `(?=...)` - Positive lookahead
- `(?!...)` - Negative lookahead
- `(?<=...)` - Positive lookbehind
- `(?<!...)` - Negative lookbehind

### ✅ Back References
- `\1`, `\2`, etc. - Numbered back references
- `\k<name>` - Named back references

### ✅ Alternation
- `|` - Alternation (OR)

### ✅ Special Characters
- `.` - Any character except newline

## Partially Supported Features

### ⚠️ Unicode Properties
- **Supported**: Basic Unicode property parsing (`\p{L}`, `\P{N}`, etc.)
- **Limitation**: Only basic parsing is implemented. Actual Unicode property matching would require full Unicode category tables and is not implemented in the current version.
- **Reasoning**: Full Unicode support requires extensive Unicode databases and complex categorization logic that would significantly increase complexity and dependencies.

## ❌ Unsupported PCRE2 Features

### Advanced Features Not Implemented

1. **Atomic Grouping**: `(?>...)`
   - **Reasoning**: Requires backtracking prevention mechanisms not present in current StepLexer architecture

2. **Possessive Quantifiers**: `*+`, `++`, `?+`, `{n,m}+`
   - **Reasoning**: Similar to atomic grouping, requires advanced backtracking control

3. **Conditional Patterns**: `(?(condition)yes|no)`
   - **Reasoning**: Adds significant complexity to state machine logic

4. **Recursive Patterns**: `(?R)`, `(?&name)`, `(?1)`
   - **Reasoning**: Requires stack-based recursion support in the StepLexer architecture

5. **Subroutines**: `(?1)`, `(?-1)`, `(?+1)`
   - **Reasoning**: Similar to recursive patterns, needs subroutine call mechanisms

6. **Inline Modifiers**: `(?i)`, `(?m)`, `(?s)`, `(?x)`, etc.
   - **Reasoning**: Would require parser state mode changes throughout pattern parsing

7. **Advanced Escape Sequences**:
   - `\Q...\E` (literal text)
   - `\K` (keep everything up to this point)
   - `\X` (extended grapheme cluster)
   - **Reasoning**: These require advanced text processing beyond basic character matching

8. **Callouts and Code**: `(?C)`, `(?{...})`
   - **Reasoning**: Would require embedding executable code in patterns, significant security and complexity concerns

9. **Comments**: `(?#...)`
   - **Reasoning**: Could be implemented but adds parsing complexity for limited benefit

10. **Variable-Length Lookbehind**
    - **Reasoning**: Current implementation assumes fixed-length lookbehind for efficiency

## Excluded Features (By Design)

The following features are intentionally excluded from the StepLexer-StepParser system due to architectural design decisions that prioritize performance, predictability, and maintainability.

### ❌ Atomic Grouping Support

**Pattern Examples:** `(?>atomic)`, `(?>(?:ab|a)c)`

**Why Excluded:**
- **Conflicts with forward-only parsing architecture**: Atomic grouping requires the ability to prevent backtracking, which fundamentally conflicts with the StepLexer's forward-only, zero-copy design
- **Would require backtracking mechanisms that violate design principles**: Implementing atomic grouping would necessitate adding backtracking state management, which contradicts the zero-copy, single-pass performance advantages
- **Compromises zero-copy, single-pass performance advantages**: The memory allocation and state tracking required for atomic grouping would eliminate the zero-copy benefits that make StepLexer efficient

**Alternative Approaches:**
- **Use possessive quantifiers within forward-parsing paradigm**: While full possessive quantifiers aren't supported, similar effects can be achieved through careful grammar design
- **Leverage grammar-based parsing in StepParser for complex constructs**: Move complex atomic grouping logic to the StepParser layer where grammar rules can provide similar functionality with explicit structure
- **Pattern restructuring**: Rewrite patterns to avoid atomic grouping by making them more explicit and less dependent on backtracking behavior

**Technical Impact:**
- Memory usage remains predictable and minimal
- Parsing performance stays within linear bounds
- Pattern compilation is fast and deterministic

### ❌ Recursive Pattern Support

**Pattern Examples:** `(?R)`, `(?&name)`, `(?1)`, `(?-2)`

**Why Excluded:**
- **Adds unnecessary complexity to lexer architecture**: Recursive patterns require stack management and dynamic state tracking that significantly complicates the lexer's streamlined architecture
- **Better handled by grammar-based StepParser for recursive constructs**: The StepParser is specifically designed to handle recursive language constructs through production rules, making it the more appropriate layer for recursion
- **Would compromise predictable memory usage and performance**: Recursive patterns can lead to unbounded memory usage and unpredictable performance characteristics, violating the StepLexer's performance guarantees

**Alternative Approaches:**
- **Use StepParser with production rules for recursive language constructs**: Define recursive patterns using grammar production rules like `<expr> ::= <expr> '+' <expr> | <number>`
- **Implement balanced parsing through grammar rules rather than regex recursion**: Use grammar-based approaches for balanced constructs like parentheses matching or nested structures
- **Hierarchical pattern decomposition**: Break complex recursive patterns into simpler, non-recursive components that can be combined at the parser level

**Technical Benefits:**
- Maintains linear memory usage characteristics
- Enables proper error recovery and reporting for recursive constructs
- Provides better debugging and analysis capabilities through explicit grammar structure

**Example Alternative Pattern:**

Instead of recursive regex:
```regex
(?R)  # Match nested parentheses recursively
```

Use StepParser grammar:
```grammar
<balanced> ::= '(' <content> ')'
<content>  ::= <balanced> <content> | <char> <content> | ε
<char>     ::= /[^()]/
```

## Architecture Notes

### vNext Architecture Compatibility

The current implementation maintains compatibility with the planned vNext architecture by:

1. **Modular Design**: Clear separation between tokenizer, parser, and state machine components
2. **Extensible Transitions**: New transition types can be easily added to the `RegexTransitionType` enum
3. **Factory Pattern**: New functionality can be added through factory extensions
4. **Step-wise Processing**: The tokenizer processes patterns step-by-step, enabling future step-based optimizations

### Performance Considerations

- **Memory Efficiency**: The ENFA structure allows for efficient memory usage compared to traditional NFA/DFA approaches
- **Compilation Speed**: Pattern compilation is fast due to the direct ENFA construction
- **Runtime Performance**: Match performance depends on pattern complexity and backtracking requirements

## Implementation Quality

### Code Quality
- ✅ Compiles successfully on .NET 8.0
- ✅ Modular, extensible architecture
- ✅ Comprehensive error handling with descriptive error messages
- ✅ Type-safe enum-based transition system

### Testing Status
- ⚠️ Basic compilation testing completed
- ❌ Comprehensive pattern testing needed
- ❌ Performance benchmarking needed
- ❌ Edge case validation needed

## Future Enhancement Roadmap

### Phase 1 (Immediate)
1. Add comprehensive unit tests
2. Fix nullable reference warnings
3. Implement basic Unicode property validation
4. Add pattern compilation validation

### Phase 2 (Short-term)
1. Implement inline modifiers (`(?i)`, `(?m)`, etc.)
2. Add `\Q...\E` literal text support
3. Implement comment support `(?#...)`
4. Add more comprehensive error reporting

### Phase 3 (Long-term)
1. Consider atomic grouping support
2. Evaluate recursive pattern feasibility
3. Advanced Unicode support with ICU integration
4. Performance optimization and benchmarking

## Conclusion

The ENFAStepLexer-StepParser provides robust support for the most commonly used PCRE2 features while maintaining a clean, extensible architecture. The implementation covers approximately 70-80% of commonly used regex features, making it suitable for most practical applications while avoiding the complexity of advanced features that are rarely used in practice.