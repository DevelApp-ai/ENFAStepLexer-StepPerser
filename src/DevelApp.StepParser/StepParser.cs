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
        public uint NodeOffset { get; set; }
        public ushort SymbolId { get; set; }
        public ushort NodeType { get; set; }
        public string RuleName { get; set; }
        public string Value { get; set; }
        public ICodeLocation Location { get; set; }

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
        public string Name { get; set; } = string.Empty;
        public List<string> RightHandSide { get; set; } = new();
        public string Context { get; set; } = string.Empty;
        public int Precedence { get; set; } = 0;
        public string Associativity { get; set; } = "none"; // none, left, right
        public Action<GraphNodeRef, List<GraphNodeRef>, CognitiveGraphBuilder>? SemanticAction { get; set; }
        public Func<ParseContext, bool>? Precondition { get; set; }

        public ProductionRule(string name, List<string> rightHandSide, string context = "")
        {
            Name = name;
            RightHandSide = rightHandSide;
            Context = context;
        }

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
        public int PathId { get; set; }
        public Stack<GraphNodeRef> ParseStack { get; set; } = new();
        public int TokenPosition { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public List<ProductionRule> ActiveProductions { get; set; } = new();
        public Dictionary<string, object> State { get; set; } = new();
        public float Score { get; set; } = 1.0f; // Path quality score
        public List<uint> NodeOffsets { get; set; } = new(); // Track node offsets for ambiguity handling

        public ParserPath(int pathId)
        {
            PathId = pathId;
        }

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
        public IContextStack ContextStack { get; set; } = new ContextStack();
        public IScopeAwareSymbolTable SymbolTable { get; set; } = new ScopeAwareSymbolTable();
        public ICodeLocation CurrentLocation { get; set; } = new CodeLocation();
        public List<StepToken> Tokens { get; set; } = new();
        public int CurrentTokenIndex { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
        public List<string> ActiveContexts { get; set; } = new();

        public StepToken? CurrentToken => CurrentTokenIndex < Tokens.Count ? Tokens[CurrentTokenIndex] : null;
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
        public List<string> Reductions { get; set; } = new();
        public List<string> ContextChanges { get; set; } = new();
        public int ActivePathCount { get; set; }
        public int CurrentPosition { get; set; }
        public bool IsComplete { get; set; }
        public List<CognitiveGraph.CognitiveGraph> CognitiveGraphs { get; set; } = new();
    }

    /// <summary>
    /// Result of processing a single parser path
    /// </summary>
    public class ParserPathResult
    {
        public List<ParserPath> NewPaths { get; set; } = new();
        public List<string> Reductions { get; set; } = new();
        public List<string> ContextChanges { get; set; } = new();
    }
}