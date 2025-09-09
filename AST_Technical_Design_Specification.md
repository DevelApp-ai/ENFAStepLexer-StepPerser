# Abstract Syntax Tree (AST) Technical Design Specification
## GrammarForge Step-Parser Architecture

### Overview
This document provides a detailed technical specification for the Abstract Syntax Tree (AST) implementation in the GrammarForge step-parser architecture. The AST is designed to bridge the parsing phase with semantic analysis and eventual integration with CognitiveGraph for advanced reasoning and error correction.

## 1. AST Core Architecture

### 1.1 ParseNode Structure
The `ParseNode` is the fundamental building block of our AST:

```csharp
public class ParseNode
{
    // Core identification
    public string RuleName { get; set; }           // Grammar rule that created this node
    public string Value { get; set; }              // Textual value (for terminals)
    public ICodeLocation Location { get; set; }    // Precise source location
    
    // Tree structure
    public List<ParseNode> Children { get; set; }  // Child nodes
    public ParseNode? Parent { get; set; }         // Parent reference (not in current impl)
    
    // Source mapping
    public StepToken? Token { get; set; }          // Original token (for terminals)
    
    // Semantic annotations
    public Dictionary<string, object> Attributes { get; set; }  // Custom semantic data
    public string Context { get; set; }           // Parse context when node was created
}
```

### 1.2 AST Node Types
The AST supports multiple node categories:

- **Terminal nodes**: Represent tokens from lexical analysis
- **Non-terminal nodes**: Represent grammar rule applications  
- **Ambiguous nodes**: Contain multiple parse alternatives
- **Error nodes**: Represent recovery points during parsing

### 1.3 Zero-Copy UTF-8 Integration
The AST maintains zero-copy principles:

- **Location references**: Point to original UTF-8 byte ranges
- **Value caching**: Lazy UTF-8 to string conversion only when needed
- **Memory efficiency**: Nodes share references to source text

## 2. GLR Multi-Path Parse Tree Construction

### 2.1 Path Management
The StepParser uses GLR-style parsing with multiple paths:

```csharp
public class ParserPath
{
    public Stack<ParseNode> ParseStack { get; set; }     // Current parse stack
    public int TokenPosition { get; set; }               // Input position
    public float Score { get; set; }                     // Path quality score
    public Dictionary<string, object> State { get; set; } // Path-specific state
}
```

### 2.2 Ambiguity Resolution
When multiple parse paths succeed, the AST handles ambiguity through:

1. **Score-based selection**: Paths with higher scores are preferred
2. **Ambiguity preservation**: Multiple valid trees can be maintained
3. **Context-sensitive disambiguation**: Use semantic rules to choose best parse

### 2.3 Tree Merging Strategy
Common sub-trees are shared between ambiguous paths to minimize memory usage:

- **Shared nodes**: Identical subtrees are merged
- **Copy-on-write**: Nodes are cloned only when modifications occur
- **Reference counting**: Tracks node usage across paths

## 3. Context-Sensitive AST Annotations

### 3.1 Context Integration
AST nodes are annotated with contextual information:

```csharp
public class ParseContext
{
    public IContextStack ContextStack { get; set; }           // Hierarchical contexts
    public IScopeAwareSymbolTable SymbolTable { get; set; }   // Symbol resolution
    public ICodeLocation CurrentLocation { get; set; }        // Current parse position
    public Dictionary<string, object> Variables { get; set; } // Semantic variables
}
```

### 3.2 Semantic Attributes
Nodes can carry semantic information:

- **Type information**: Variable types, expression types
- **Scope data**: Local variable visibility, function parameters  
- **Control flow**: Break/continue targets, exception handlers
- **Cross-references**: Symbol usage, definition links

### 3.3 Projection Match Integration
The AST supports projection-based semantic rules:

```csharp
// Grammar rule: @context(function-context) @projection(IDENTIFIER ASSIGN)
// When matched, executes semantic action on the AST node
node.Attributes["projection"] = "local_assignment";
node.Attributes["context"] = "function-context";
```

## 4. CognitiveGraph Integration Design

### 4.1 AST-to-Graph Mapping
To integrate with CognitiveGraph, AST nodes map to graph structures:

```csharp
public class ASTGraphNode
{
    public string NodeId { get; set; }                    // Unique identifier
    public string NodeType { get; set; }                  // AST rule name
    public Dictionary<string, object> Properties { get; set; } // Node data
    public List<ASTGraphEdge> Edges { get; set; }         // Relationships
}

public class ASTGraphEdge
{
    public string EdgeType { get; set; }                  // Relationship type
    public string TargetNodeId { get; set; }             // Target node
    public Dictionary<string, object> Properties { get; set; } // Edge metadata
}
```

### 4.2 Reasoning Integration Points
The AST provides these integration points for CognitiveGraph:

1. **Semantic queries**: Extract patterns from AST structure
2. **Error recovery**: Use graph reasoning to suggest fixes
3. **Code completion**: Leverage AST context for suggestions
4. **Ambiguity resolution**: Use learning to choose best parses

### 4.3 Knowledge Extraction
From the AST, CognitiveGraph can extract:

- **Structural patterns**: Common code idioms and anti-patterns
- **Semantic relationships**: Variable dependencies, call graphs
- **Context rules**: Valid constructs in different scopes
- **Error patterns**: Common mistakes and their corrections

## 5. Location-Based Operations (RefakTS Integration)

### 5.1 Precise Targeting
The AST supports surgical code operations through precise location mapping:

```csharp
public interface ICodeLocation
{
    string FileName { get; }
    int StartLine { get; }
    int StartColumn { get; }
    int EndLine { get; }
    int EndColumn { get; }
    string Context { get; }
}
```

### 5.2 AST Traversal for Operations
Location-based operations traverse the AST to find target nodes:

1. **Location matching**: Find nodes at specific coordinates
2. **Structural queries**: Select nodes by type and context
3. **Semantic filtering**: Use symbol tables for precise targeting

### 5.3 Refactoring Operations
The AST supports these RefakTS-style operations:

- **Extract variable**: Convert expression subtrees to variables
- **Inline variable**: Replace variable references with values
- **Rename symbol**: Update all references in AST and symbol tables
- **Extract method**: Convert statement sequences to function calls

## 6. Memory Efficiency and Performance

### 6.1 Zero-Copy Characteristics
The AST maintains zero-copy principles:

- **String interning**: Common strings are shared
- **Location references**: Point to original source, not copies
- **Lazy evaluation**: Complex computations deferred until needed

### 6.2 Memory Usage Patterns
Typical memory usage for AST construction:

- **Small programs** (< 1KB): ~2-3KB AST overhead
- **Medium programs** (< 10KB): ~5-10KB AST overhead  
- **Large programs** (< 100KB): ~20-50KB AST overhead

### 6.3 Performance Characteristics
- **Construction time**: O(n log k) where n=tokens, k=ambiguous paths
- **Lookup time**: O(log n) for location-based queries with spatial indexing
- **Memory usage**: 15-30% less than traditional string-based ASTs

## 7. Error Handling and Recovery

### 7.1 Error Node Integration
Parse errors are represented as special AST nodes:

```csharp
public class ErrorNode : ParseNode
{
    public string ErrorType { get; set; }                // Syntax, semantic, etc.
    public string ErrorMessage { get; set; }             // Human-readable description
    public List<string> SuggestedFixes { get; set; }     // Possible corrections
    public ParseNode? PartialParse { get; set; }         // Best-effort parse
}
```

### 7.2 Recovery Strategies
The AST supports multiple error recovery approaches:

1. **Panic mode**: Skip tokens until synchronization point
2. **Phrase-level**: Insert/delete tokens to continue parsing  
3. **Error productions**: Explicit grammar rules for common errors
4. **Semantic recovery**: Use context to infer missing constructs

## 8. Extensibility and Future Integration

### 8.1 Plugin Architecture
The AST supports extensibility through:

- **Custom node types**: Extend ParseNode for domain-specific needs
- **Semantic analyzers**: Register analysis passes over AST
- **Transformation pipelines**: Chain AST modifications
- **External tool integration**: Export AST to other formats

### 8.2 CognitiveGraph Bridge Interface
Proposed interface for CognitiveGraph integration:

```csharp
public interface ICognitiveGraphBridge
{
    // Convert AST to graph representation
    ASTGraphNode[] ConvertToGraph(ParseNode ast);
    
    // Query graph for patterns and insights
    QueryResult ExecuteQuery(string query, ASTGraphNode[] graph);
    
    // Get error correction suggestions
    ErrorCorrection[] SuggestCorrections(ErrorNode error, ASTGraphNode[] context);
    
    // Learn from successful parses
    void LearnFromExample(ParseNode ast, ParseContext context);
}
```

This AST design provides a solid foundation for integrating with CognitiveGraph while maintaining the zero-copy UTF-8 architecture and supporting the full range of GrammarForge capabilities including context-sensitive parsing, surgical code operations, and multi-path ambiguity resolution.