using System;
using System.Collections.Generic;
using System.Linq;
using DevelApp.SplitLexer;  // Keep old namespace until we fix it
using CognitiveGraph;

namespace DevelApp.StepParser
{
    // TODO: Replace ParseNode with CognitiveGraph integration
    // Keeping ParseNode temporarily until we understand CognitiveGraph API

    /// <summary>
    /// CognitiveGraph integration wrapper - to be implemented with actual CognitiveGraph types
    /// </summary>
    public interface ICognitiveGraphIntegration
    {
        // TODO: Replace with actual CognitiveGraph types once API is known
        object CreateGraph();
        object CreateNode(string type, string value, ICodeLocation location);
        void AddNodeToGraph(object graph, object node);
        void ConnectNodes(object parentNode, object childNode);
        object GetBestParseResult(object graph);
    }

    /// <summary>
    /// Parse tree node for AST construction
    /// NOTE: This will be replaced with CognitiveGraph nodes
    /// </summary>
    public class ParseNode
    {
        public string RuleName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ICodeLocation Location { get; set; } = new CodeLocation();
        public List<ParseNode> Children { get; set; } = new();
        public StepToken? Token { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();

        public ParseNode() { }

        public ParseNode(string ruleName, ICodeLocation location)
        {
            RuleName = ruleName;
            Location = location;
        }

        public ParseNode(StepToken token)
        {
            RuleName = token.Type;
            Value = token.Value;
            Location = token.Location;
            Token = token;
        }

        public override string ToString()
        {
            return $"{RuleName}: {Value} [{Children.Count} children]";
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
        public Action<ParseNode, List<ParseNode>>? SemanticAction { get; set; }
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
    /// Parser path for GLR-style multi-path parsing
    /// </summary>
    public class ParserPath
    {
        public int PathId { get; set; }
        public Stack<ParseNode> ParseStack { get; set; } = new();
        public int TokenPosition { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public List<ProductionRule> ActiveProductions { get; set; } = new();
        public Dictionary<string, object> State { get; set; } = new();
        public float Score { get; set; } = 1.0f; // Path quality score

        public ParserPath(int pathId)
        {
            PathId = pathId;
        }

        public ParserPath Clone(int newPathId)
        {
            var clonedStack = new Stack<ParseNode>();
            var tempList = new List<ParseNode>(ParseStack.Reverse());
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
                Score = Score
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
    /// Step parser with GLR-style multi-path processing for ambiguity resolution
    /// </summary>
    public class StepParser
    {
        private readonly List<ProductionRule> _grammar = new();
        private readonly List<ParserPath> _activePaths = new();
        private readonly ParseContext _context = new();
        private int _nextPathId = 0;

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
        public void Initialize(List<StepToken> tokens)
        {
            _context.Tokens = tokens;
            _context.CurrentTokenIndex = 0;
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
                result.ParseTrees = GenerateCompleteParseTrees();
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
                var tokenNode = new ParseNode(token);
                newPath.ParseStack.Push(tokenNode);
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
            var children = new List<ParseNode>();
            for (int i = 0; i < rule.RightHandSide.Count; i++)
            {
                if (newPath.ParseStack.Count > 0)
                {
                    children.Insert(0, newPath.ParseStack.Pop());
                }
                else
                {
                    return null; // Invalid reduction
                }
            }

            // Create new node for LHS
            var location = children.Count > 0 ? children[0].Location : new CodeLocation();
            var newNode = new ParseNode(rule.Name, location)
            {
                Children = children
            };

            // Execute semantic action if present
            try
            {
                rule.SemanticAction?.Invoke(newNode, children);
            }
            catch (Exception ex)
            {
                // Log semantic action error and continue
                Console.WriteLine($"Semantic action error for rule {rule.Name}: {ex.Message}");
            }

            newPath.ParseStack.Push(newNode);
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
        /// Generate complete parse trees from successful paths
        /// </summary>
        private List<ParseNode> GenerateCompleteParseTrees()
        {
            var completeTrees = new List<ParseNode>();

            foreach (var path in _activePaths.Where(p => p.IsValid && p.ParseStack.Count == 1))
            {
                completeTrees.Add(path.ParseStack.Peek());
            }

            return completeTrees;
        }

        /// <summary>
        /// Select the best parse tree based on path scores
        /// </summary>
        public ParseNode? SelectBestParseTree()
        {
            var completeTrees = GenerateCompleteParseTrees();
            if (!completeTrees.Any())
                return null;

            // Find the path with highest score that produced a complete parse
            var bestPath = _activePaths
                .Where(p => p.IsValid && p.ParseStack.Count == 1)
                .OrderByDescending(p => p.Score)
                .FirstOrDefault();

            return bestPath?.ParseStack.Peek();
        }

        /// <summary>
        /// Handle ambiguous parses by returning multiple valid trees
        /// </summary>
        public List<ParseNode> HandleAmbiguity()
        {
            var ambiguousTrees = GenerateCompleteParseTrees();
            
            // If multiple complete parses exist, they represent ambiguity
            if (ambiguousTrees.Count > 1)
            {
                // Apply disambiguation heuristics
                return DisambiguateParseTrees(ambiguousTrees);
            }

            return ambiguousTrees;
        }

        /// <summary>
        /// Apply disambiguation heuristics to multiple parse trees
        /// </summary>
        private List<ParseNode> DisambiguateParseTrees(List<ParseNode> trees)
        {
            // Example disambiguation strategies:
            // 1. Prefer trees with fewer nodes (simpler interpretations)
            // 2. Prefer trees that follow precedence rules
            // 3. Use semantic constraints

            return trees
                .OrderBy(tree => CountNodes(tree)) // Prefer simpler trees
                .ToList();
        }

        /// <summary>
        /// Count total nodes in parse tree
        /// </summary>
        private int CountNodes(ParseNode node)
        {
            return 1 + node.Children.Sum(child => CountNodes(child));
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
        public List<ParseNode> ParseTrees { get; set; } = new();
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