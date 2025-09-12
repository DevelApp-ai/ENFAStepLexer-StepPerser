using System;
using System.Collections.Generic;
using System.Linq;
using DevelApp.StepLexer;
using CognitiveGraph;
using CognitiveGraph.Builder;
using CognitiveGraph.Accessors;
using CognitiveGraph.Schema;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Node reference in the CognitiveGraph for parser paths
    /// </summary>
    public struct GraphNodeRef
    {
        /// <summary>Gets or sets the offset of the node in the graph.</summary>
        public uint NodeOffset { get; set; }
        
        /// <summary>Gets or sets the symbol identifier for this node.</summary>
        public ushort SymbolId { get; set; }
        
        /// <summary>Gets or sets the type of the node.</summary>
        public ushort NodeType { get; set; }
        
        /// <summary>Gets or sets the name of the production rule this node represents.</summary>
        public string RuleName { get; set; }
        
        /// <summary>Gets or sets the value associated with this node.</summary>
        public string Value { get; set; }
        
        /// <summary>Gets or sets the location in the source code for this node.</summary>
        public ICodeLocation Location { get; set; }

        /// <summary>
        /// Initializes a new instance of the GraphNodeRef struct.
        /// </summary>
        /// <param name="nodeOffset">The offset of the node in the graph</param>
        /// <param name="symbolId">The symbol identifier for this node</param>
        /// <param name="nodeType">The type of the node</param>
        /// <param name="ruleName">The name of the production rule this node represents</param>
        /// <param name="value">The value associated with this node</param>
        /// <param name="location">The location in the source code for this node</param>
        public GraphNodeRef(uint nodeOffset, ushort symbolId, ushort nodeType, string ruleName, string value, ICodeLocation location)
        {
            NodeOffset = nodeOffset;
            SymbolId = symbolId;
            NodeType = nodeType;
            RuleName = ruleName;
            Value = value;
            Location = location;
        }
    }

    /// <summary>
    /// Grammar production rule for parsing
    /// </summary>
    public class ProductionRule
    {
        /// <summary>Gets or sets the name of the production rule.</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the right-hand side symbols of the production rule.</summary>
        public List<string> RightHandSide { get; set; } = new();
        
        /// <summary>Gets or sets the context in which this rule applies.</summary>
        public string Context { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the precedence level for this rule.</summary>
        public int Precedence { get; set; } = 0;
        
        /// <summary>Gets or sets the associativity of this rule (none, left, right).</summary>
        public string Associativity { get; set; } = "none"; // none, left, right
        
        /// <summary>Gets or sets the semantic action to execute when this rule is applied.</summary>
        public Action<GraphNodeRef, List<GraphNodeRef>, CognitiveGraphBuilder>? SemanticAction { get; set; }
        
        /// <summary>Gets or sets the precondition that must be satisfied for this rule to apply.</summary>
        public Func<ParseContext, bool>? Precondition { get; set; }

        /// <summary>
        /// Initializes a new instance of the ProductionRule class.
        /// </summary>
        /// <param name="name">The name of the production rule</param>
        /// <param name="rightHandSide">The right-hand side symbols of the production rule</param>
        /// <param name="context">The context in which this rule applies (optional)</param>
        public ProductionRule(string name, List<string> rightHandSide, string context = "")
        {
            Name = name;
            RightHandSide = rightHandSide;
            Context = context;
        }

        /// <summary>
        /// Returns a string representation of the production rule.
        /// </summary>
        /// <returns>A string in the format "Name ::= RightHandSide"</returns>
        public override string ToString()
        {
            return $"{Name} ::= {string.Join(" ", RightHandSide)}";
        }
    }

    /// <summary>
    /// Parser path for GLR-style multi-path parsing with CognitiveGraph integration
    /// </summary>
    public class ParserPath
    {
        /// <summary>Gets or sets the unique identifier for this parse path.</summary>
        public int PathId { get; set; }
        
        /// <summary>Gets or sets the parse stack containing graph node references.</summary>
        public Stack<GraphNodeRef> ParseStack { get; set; } = new();
        
        /// <summary>Gets or sets the current position in the token stream.</summary>
        public int TokenPosition { get; set; }
        
        /// <summary>Gets or sets the current parser state.</summary>
        public string CurrentState { get; set; } = string.Empty;
        
        /// <summary>Gets or sets whether this parse path is still valid.</summary>
        public bool IsValid { get; set; } = true;
        
        /// <summary>Gets or sets the list of production rules currently being processed.</summary>
        public List<ProductionRule> ActiveProductions { get; set; } = new();
        
        /// <summary>Gets or sets the state dictionary for parser context information.</summary>
        public Dictionary<string, object> State { get; set; } = new();
        
        /// <summary>Gets or sets the quality score for this parse path.</summary>
        public float Score { get; set; } = 1.0f; // Path quality score
        
        /// <summary>Gets or sets the list of node offsets for ambiguity handling.</summary>
        public List<uint> NodeOffsets { get; set; } = new(); // Track node offsets for ambiguity handling

        /// <summary>
        /// Initializes a new instance of the ParserPath class.
        /// </summary>
        /// <param name="pathId">The unique identifier for this parse path</param>
        public ParserPath(int pathId)
        {
            PathId = pathId;
        }

        /// <summary>
        /// Creates a deep copy of this parser path with a new path ID.
        /// </summary>
        /// <param name="newPathId">The unique identifier for the cloned path</param>
        /// <returns>A new ParserPath instance with copied state</returns>
        public ParserPath Clone(int newPathId)
        {
            var clonedStack = new Stack<GraphNodeRef>();
            var tempList = new List<GraphNodeRef>(ParseStack.Reverse());
            tempList.Reverse();
            foreach (var node in tempList)
            {
                clonedStack.Push(node);
            }

            return new ParserPath(newPathId)
            {
                ParseStack = clonedStack,
                TokenPosition = TokenPosition,
                CurrentState = CurrentState,
                IsValid = IsValid,
                ActiveProductions = new List<ProductionRule>(ActiveProductions),
                State = new Dictionary<string, object>(State),
                Score = Score,
                NodeOffsets = new List<uint>(NodeOffsets)
            };
        }
    }

    /// <summary>
    /// Parse context for semantic actions and rule evaluation
    /// </summary>
    public class ParseContext
    {
        /// <summary>Gets or sets the hierarchical context stack for scope management.</summary>
        public IContextStack ContextStack { get; set; } = new ContextStack();
        
        /// <summary>Gets or sets the scope-aware symbol table for identifier resolution.</summary>
        public IScopeAwareSymbolTable SymbolTable { get; set; } = new ScopeAwareSymbolTable();
        
        /// <summary>Gets or sets the current location in the source code being parsed.</summary>
        public ICodeLocation CurrentLocation { get; set; } = new CodeLocation();
        
        /// <summary>Gets or sets the list of tokens being parsed.</summary>
        public List<StepToken> Tokens { get; set; } = new();
        
        /// <summary>Gets or sets the current index in the token stream.</summary>
        public int CurrentTokenIndex { get; set; }
        
        /// <summary>Gets or sets the variables dictionary for context-specific data.</summary>
        public Dictionary<string, object> Variables { get; set; } = new();
        
        /// <summary>Gets or sets the list of currently active parsing contexts.</summary>
        public List<string> ActiveContexts { get; set; } = new();

        /// <summary>Gets the current token being processed, or null if at end of stream.</summary>
        public StepToken? CurrentToken => CurrentTokenIndex < Tokens.Count ? Tokens[CurrentTokenIndex] : null;
        
        /// <summary>
        /// Gets a look-ahead token at the specified offset from the current position.
        /// </summary>
        /// <param name="offset">The number of tokens to look ahead (default: 1)</param>
        /// <returns>The token at the look-ahead position, or null if beyond end of stream</returns>
        public StepToken? LookAhead(int offset = 1) => (CurrentTokenIndex + offset) < Tokens.Count ? Tokens[CurrentTokenIndex + offset] : null;
    }

    /// <summary>
    /// Step parser with GLR-style multi-path processing and CognitiveGraph integration
    /// </summary>
    public class StepParser : IDisposable
    {
        private readonly List<ProductionRule> _grammar = new();
        private readonly List<ParserPath> _activePaths = new();
        private readonly ParseContext _context = new();
        private readonly CognitiveGraphBuilder _graphBuilder = new();
        private int _nextPathId = 0;
        private ushort _nextSymbolId = 1;
        private string _sourceText = "";

        /// <summary>
        /// Active parsing paths
        /// </summary>
        public IReadOnlyList<ParserPath> ActivePaths => _activePaths;

        /// <summary>
        /// Current parse context
        /// </summary>
        public ParseContext Context => _context;

        /// <summary>
        /// Add a production rule to the grammar
        /// </summary>
        public void AddRule(ProductionRule rule)
        {
            _grammar.Add(rule);
        }

        /// <summary>
        /// Initialize parser with tokens
        /// </summary>
        public void Initialize(List<StepToken> tokens, string sourceText = "")
        {
            _context.Tokens = tokens;
            _context.CurrentTokenIndex = 0;
            _sourceText = sourceText;
            _activePaths.Clear();
            _activePaths.Add(new ParserPath(_nextPathId++));
        }

        /// <summary>
        /// Parse next token and return results
        /// </summary>
        public ParserStepResult Step()
        {
            var result = new ParserStepResult();

            if (_context.CurrentToken == null)
            {
                result.IsComplete = true;
                result.CognitiveGraphs = GenerateCompleteCognitiveGraphs();
                return result;
            }

            var newPaths = new List<ParserPath>();

            foreach (var path in _activePaths.Where(p => p.IsValid))
            {
                var pathResults = ProcessParserPath(path, _context.CurrentToken);
                newPaths.AddRange(pathResults.NewPaths);
                result.Reductions.AddRange(pathResults.Reductions);
                
                if (pathResults.ContextChanges.Any())
                {
                    result.ContextChanges.AddRange(pathResults.ContextChanges);
                }
            }

            // Update paths and merge identical ones
            _activePaths.Clear();
            _activePaths.AddRange(MergeParserPaths(newPaths));

            // Prune low-quality paths if too many exist
            if (_activePaths.Count > 10)
            {
                _activePaths.Sort((a, b) => b.Score.CompareTo(a.Score));
                _activePaths.RemoveRange(10, _activePaths.Count - 10);
            }

            _context.CurrentTokenIndex++;
            result.ActivePathCount = _activePaths.Count;
            result.CurrentPosition = _context.CurrentTokenIndex;

            return result;
        }

        /// <summary>
        /// Process a single parser path
        /// </summary>
        private ParserPathResult ProcessParserPath(ParserPath path, StepToken currentToken)
        {
            var result = new ParserPathResult();

            // Try to shift (accept current token)
            var shiftResult = TryShift(path, currentToken);
            if (shiftResult.success)
            {
                result.NewPaths.Add(shiftResult.path);
            }

            // Try to reduce (apply production rules)
            var reductionResults = TryReduce(path, currentToken);
            result.NewPaths.AddRange(reductionResults.Select(r => r.path));
            result.Reductions.AddRange(reductionResults.Select(r => r.reduction));

            // If no actions possible, mark path as invalid
            if (!shiftResult.success && !reductionResults.Any())
            {
                path.IsValid = false;
                result.NewPaths.Add(path);
            }

            return result;
        }

        /// <summary>
        /// Try to shift current token onto parse stack
        /// </summary>
        private (bool success, ParserPath path) TryShift(ParserPath path, StepToken token)
        {
            // Check if we can accept this token type
            if (CanAcceptToken(path, token))
            {
                var newPath = path.Clone(_nextPathId++);
                
                // Create node in CognitiveGraph
                var properties = new List<(string key, PropertyValueType type, object value)>
                {
                    ("TokenType", PropertyValueType.String, token.Type),
                    ("TokenValue", PropertyValueType.String, token.Value),
                    ("Context", PropertyValueType.String, token.Context),
                    ("IsTerminal", PropertyValueType.Boolean, true)
                };

                var nodeOffset = _graphBuilder.WriteSymbolNode(
                    symbolId: _nextSymbolId++,
                    nodeType: 100, // Terminal node type
                    sourceStart: (uint)token.Location.StartColumn, // Use column as position
                    sourceLength: (uint)token.Value.Length,
                    properties: properties
                );

                var nodeRef = new GraphNodeRef(
                    nodeOffset, 
                    (ushort)(_nextSymbolId - 1),
                    100,
                    token.Type, 
                    token.Value, 
                    token.Location
                );

                newPath.ParseStack.Push(nodeRef);
                newPath.NodeOffsets.Add(nodeOffset);
                newPath.TokenPosition++;
                newPath.Score *= 0.95f; // Slight penalty for each shift
                return (true, newPath);
            }

            return (false, path);
        }

        /// <summary>
        /// Try to reduce using available production rules
        /// </summary>
        private List<(ParserPath path, string reduction)> TryReduce(ParserPath path, StepToken currentToken)
        {
            var results = new List<(ParserPath path, string reduction)>();

            foreach (var rule in _grammar)
            {
                if (CanApplyReduction(path, rule, currentToken))
                {
                    var reducedPath = ApplyReduction(path, rule);
                    if (reducedPath != null)
                    {
                        results.Add((reducedPath, rule.ToString()));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Check if we can accept a token in the current parse state
        /// </summary>
        private bool CanAcceptToken(ParserPath path, StepToken token)
        {
            // Look for rules that expect this token type
            return _grammar.Any(rule => 
                rule.RightHandSide.Contains(token.Type) && 
                IsRuleApplicableInContext(rule, token.Context) &&
                (rule.Precondition?.Invoke(_context) ?? true));
        }

        /// <summary>
        /// Check if a reduction can be applied
        /// </summary>
        private bool CanApplyReduction(ParserPath path, ProductionRule rule, StepToken currentToken)
        {
            if (path.ParseStack.Count < rule.RightHandSide.Count)
                return false;

            if (!IsRuleApplicableInContext(rule, currentToken.Context))
                return false;

            if (rule.Precondition != null && !rule.Precondition(_context))
                return false;

            // Check if top stack elements match rule RHS (in reverse order)
            var stackItems = path.ParseStack.Take(rule.RightHandSide.Count).ToArray();
            for (int i = 0; i < rule.RightHandSide.Count; i++)
            {
                var expectedType = rule.RightHandSide[rule.RightHandSide.Count - 1 - i];
                var actualType = stackItems[i].RuleName;
                
                if (expectedType != actualType)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Apply a production rule reduction
        /// </summary>
        private ParserPath? ApplyReduction(ParserPath path, ProductionRule rule)
        {
            var newPath = path.Clone(_nextPathId++);
            
            // Pop RHS elements from stack
            var children = new List<GraphNodeRef>();
            var childNodeOffsets = new List<uint>();
            for (int i = 0; i < rule.RightHandSide.Count; i++)
            {
                if (newPath.ParseStack.Count > 0)
                {
                    var childRef = newPath.ParseStack.Pop();
                    children.Insert(0, childRef);
                    childNodeOffsets.Insert(0, childRef.NodeOffset);
                }
                else
                {
                    return null; // Invalid reduction
                }
            }

            // Determine source span for the new node
            var location = children.Count > 0 ? children[0].Location : new CodeLocation();
            var sourceStart = children.Count > 0 ? (uint)children[0].Location.StartColumn : 0u;
            var sourceEnd = children.Count > 0 ? (uint)children.Last().Location.EndColumn : 0u;
            var sourceLength = sourceEnd > sourceStart ? sourceEnd - sourceStart : 0u;

            // Create properties for the non-terminal node
            var properties = new List<(string key, PropertyValueType type, object value)>
            {
                ("RuleName", PropertyValueType.String, rule.Name),
                ("Context", PropertyValueType.String, rule.Context),
                ("IsTerminal", PropertyValueType.Boolean, false),
                ("Precedence", PropertyValueType.Int32, rule.Precedence),
                ("Associativity", PropertyValueType.String, rule.Associativity)
            };

            // Create packed node for the reduction if there are children
            uint packedNodeOffset = 0;
            if (childNodeOffsets.Any())
            {
                packedNodeOffset = _graphBuilder.WritePackedNode(
                    ruleId: (ushort)(_grammar.IndexOf(rule) + 1),
                    childNodeOffsets: childNodeOffsets
                );
            }

            // Create the symbol node
            var packedNodes = packedNodeOffset != 0 ? new List<uint> { packedNodeOffset } : null;
            var nodeOffset = _graphBuilder.WriteSymbolNode(
                symbolId: _nextSymbolId++,
                nodeType: 200, // Non-terminal node type
                sourceStart: sourceStart,
                sourceLength: sourceLength,
                packedNodeOffsets: packedNodes,
                properties: properties
            );

            var newNodeRef = new GraphNodeRef(
                nodeOffset,
                (ushort)(_nextSymbolId - 1),
                200,
                rule.Name,
                "",
                location
            );

            // Execute semantic action if present
            try
            {
                rule.SemanticAction?.Invoke(newNodeRef, children, _graphBuilder);
            }
            catch (Exception ex)
            {
                // Log semantic action error and continue
                Console.WriteLine($"Semantic action error for rule {rule.Name}: {ex.Message}");
            }

            newPath.ParseStack.Push(newNodeRef);
            newPath.NodeOffsets.Add(nodeOffset);
            newPath.Score *= 1.1f; // Reward successful reductions
            
            return newPath;
        }

        /// <summary>
        /// Check if rule is applicable in current context
        /// </summary>
        private bool IsRuleApplicableInContext(ProductionRule rule, string currentContext)
        {
            if (string.IsNullOrEmpty(rule.Context))
                return true;

            return rule.Context == currentContext || _context.ContextStack.Contains(rule.Context);
        }

        /// <summary>
        /// Merge similar parser paths to reduce explosion
        /// </summary>
        private List<ParserPath> MergeParserPaths(List<ParserPath> paths)
        {
            var merged = new Dictionary<string, ParserPath>();

            foreach (var path in paths.Where(p => p.IsValid))
            {
                var key = GeneratePathKey(path);
                if (!merged.ContainsKey(key) || merged[key].Score < path.Score)
                {
                    merged[key] = path;
                }
            }

            return merged.Values.ToList();
        }

        /// <summary>
        /// Generate a key for path merging
        /// </summary>
        private string GeneratePathKey(ParserPath path)
        {
            var stackSignature = string.Join(",", path.ParseStack.Select(n => n.RuleName));
            return $"{path.TokenPosition}:{path.CurrentState}:{stackSignature}";
        }

        /// <summary>
        /// Generate complete CognitiveGraphs from successful paths
        /// </summary>
        private List<CognitiveGraph.CognitiveGraph> GenerateCompleteCognitiveGraphs()
        {
            var completeGraphs = new List<CognitiveGraph.CognitiveGraph>();

            var successfulPaths = _activePaths.Where(p => p.IsValid && p.ParseStack.Count == 1).ToList();
            
            if (successfulPaths.Count == 1)
            {
                // Single successful parse - create simple graph
                var path = successfulPaths[0];
                var rootNodeRef = path.ParseStack.Peek();
                var buffer = _graphBuilder.Build(rootNodeRef.NodeOffset, _sourceText);
                completeGraphs.Add(new CognitiveGraph.CognitiveGraph(buffer));
            }
            else if (successfulPaths.Count > 1)
            {
                // Multiple successful parses - create ambiguous graph with packed nodes
                var ambiguousNodeOffsets = successfulPaths.Select(p => p.ParseStack.Peek().NodeOffset).ToList();
                
                // Create packed nodes for each interpretation
                var packedNodeOffsets = new List<uint>();
                for (int i = 0; i < successfulPaths.Count; i++)
                {
                    var packedNodeOffset = _graphBuilder.WritePackedNode(
                        ruleId: (ushort)(i + 1),
                        childNodeOffsets: new List<uint> { ambiguousNodeOffsets[i] }
                    );
                    packedNodeOffsets.Add(packedNodeOffset);
                }

                // Create ambiguous root node
                var ambiguousProperties = new List<(string key, PropertyValueType type, object value)>
                {
                    ("NodeType", PropertyValueType.String, "AmbiguousRoot"),
                    ("ParseCount", PropertyValueType.Int32, successfulPaths.Count),
                    ("IsAmbiguous", PropertyValueType.Boolean, true)
                };

                var ambiguousRootOffset = _graphBuilder.WriteSymbolNode(
                    symbolId: _nextSymbolId++,
                    nodeType: 300, // Ambiguous root node type
                    sourceStart: 0,
                    sourceLength: (uint)_sourceText.Length,
                    packedNodeOffsets: packedNodeOffsets,
                    properties: ambiguousProperties
                );

                var buffer = _graphBuilder.Build(ambiguousRootOffset, _sourceText);
                completeGraphs.Add(new CognitiveGraph.CognitiveGraph(buffer));
            }

            return completeGraphs;
        }

        /// <summary>
        /// Select the best CognitiveGraph based on path scores
        /// </summary>
        public CognitiveGraph.CognitiveGraph? SelectBestParseGraph()
        {
            var completeGraphs = GenerateCompleteCognitiveGraphs();
            if (!completeGraphs.Any())
                return null;

            // For now, return the first available graph
            // In the future, we could implement scoring based on graph properties
            return completeGraphs.First();
        }

        /// <summary>
        /// Handle ambiguous parses by returning CognitiveGraph with packed nodes
        /// </summary>
        public List<CognitiveGraph.CognitiveGraph> HandleAmbiguity()
        {
            return GenerateCompleteCognitiveGraphs();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _graphBuilder?.Dispose();
        }
    }

    /// <summary>
    /// Result of a single parser step
    /// </summary>
    public class ParserStepResult
    {
        /// <summary>Gets or sets the list of reductions performed during this step.</summary>
        public List<string> Reductions { get; set; } = new();
        
        /// <summary>Gets or sets the list of context changes that occurred during this step.</summary>
        public List<string> ContextChanges { get; set; } = new();
        
        /// <summary>Gets or sets the number of active parse paths after this step.</summary>
        public int ActivePathCount { get; set; }
        
        /// <summary>Gets or sets the current position in the token stream after this step.</summary>
        public int CurrentPosition { get; set; }
        
        /// <summary>Gets or sets whether the parsing is complete after this step.</summary>
        public bool IsComplete { get; set; }
        
        /// <summary>Gets or sets the list of CognitiveGraphs representing parse results.</summary>
        public List<CognitiveGraph.CognitiveGraph> CognitiveGraphs { get; set; } = new();
    }

    /// <summary>
    /// Result of processing a single parser path
    /// </summary>
    public class ParserPathResult
    {
        /// <summary>Gets or sets the list of new parser paths created during processing.</summary>
        public List<ParserPath> NewPaths { get; set; } = new();
        
        /// <summary>Gets or sets the list of reductions performed on this path.</summary>
        public List<string> Reductions { get; set; } = new();
        
        /// <summary>Gets or sets the list of context changes that occurred on this path.</summary>
        public List<string> ContextChanges { get; set; } = new();
    }
}