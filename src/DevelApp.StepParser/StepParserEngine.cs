using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DevelApp.StepLexer;
using CognitiveGraph;
using CognitiveGraph.Accessors;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Selection criteria for location-based targeting
    /// </summary>
    public class SelectionCriteria
    {
        /// <summary>Gets or sets the regex pattern for matching target locations.</summary>
        public string? Regex { get; set; }
        
        /// <summary>Gets or sets the range specification with start and end markers.</summary>
        public (string start, string end)? Range { get; set; }
        
        /// <summary>Gets or sets the structural selection with type and member inclusion options.</summary>
        public (string type, bool includeFields, bool includeMethods)? Structural { get; set; }
        
        /// <summary>Gets or sets the boundary specification for selection limits.</summary>
        public string? Boundaries { get; set; }
        
        /// <summary>Gets or sets the grammar-based selection criteria.</summary>
        public string? Grammar { get; set; }
    }

    /// <summary>
    /// Refactoring operation definition
    /// </summary>
    public class RefactoringOperation
    {
        /// <summary>Gets or sets the name of the refactoring operation.</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the applicable contexts where this operation can be used.</summary>
        public string[] ApplicableContexts { get; set; } = Array.Empty<string>();
        
        /// <summary>Gets or sets the preconditions function that determines if the operation can be applied.</summary>
        public Func<ParseContext, bool>? Preconditions { get; set; }
        
        /// <summary>Gets or sets the execution function that performs the refactoring operation.</summary>
        public Func<ICodeLocation, ParseContext, RefactoringResult>? Execute { get; set; }
        
        /// <summary>Gets or sets the description of what the refactoring operation does.</summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of a refactoring operation
    /// </summary>
    public class RefactoringResult
    {
        /// <summary>Gets or sets whether the refactoring operation was successful.</summary>
        public bool Success { get; set; }
        
        /// <summary>Gets or sets the message describing the result of the operation.</summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the list of code changes made during the refactoring.</summary>
        public List<CodeChange> Changes { get; set; } = new();
        
        /// <summary>Gets or sets the location of the modified node after refactoring.</summary>
        public ICodeLocation? ModifiedNodeLocation { get; set; }
    }

    /// <summary>
    /// Individual code change for surgical operations
    /// </summary>
    public class CodeChange
    {
        /// <summary>Gets or sets the location where the code change is applied.</summary>
        public ICodeLocation Location { get; set; } = new CodeLocation();
        
        /// <summary>Gets or sets the original text before the change.</summary>
        public string OriginalText { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the new text after the change.</summary>
        public string NewText { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the type of change (insert, delete, replace).</summary>
        public string ChangeType { get; set; } = string.Empty; // insert, delete, replace
    }

    /// <summary>
    /// Complete step-parsing result with CognitiveGraph integration
    /// </summary>
    public class StepParsingResult
    {
        /// <summary>Gets or sets whether the parsing operation was successful.</summary>
        public bool Success { get; set; }
        
        /// <summary>Gets or sets the primary CognitiveGraph result from parsing.</summary>
        public CognitiveGraph.CognitiveGraph? CognitiveGraph { get; set; }
        
        /// <summary>Gets or sets the list of ambiguous parse results when multiple interpretations are possible.</summary>
        public List<CognitiveGraph.CognitiveGraph> AmbiguousParses { get; set; } = new();
        
        /// <summary>Gets or sets the tokens generated during parsing.</summary>
        public List<StepToken> Tokens { get; set; } = new();
        
        /// <summary>Gets or sets the list of errors encountered during parsing.</summary>
        public List<string> Errors { get; set; } = new();
        
        /// <summary>Gets or sets the time taken to complete the parsing operation.</summary>
        public TimeSpan ParseTime { get; set; }
        
        /// <summary>Gets or sets the number of parse paths explored during ambiguity resolution.</summary>
        public int PathCount { get; set; }
        
        /// <summary>Gets or sets the parsing context with scope and semantic information.</summary>
        public ParseContext Context { get; set; } = new ParseContext();
    }

    /// <summary>
    /// Main step-parser engine coordinating lexer, parser, and semantic analysis
    /// Implements the GrammarForge architecture with location-based targeting and surgical operations
    /// </summary>
    public class StepParserEngine : IDisposable
    {
        private readonly DevelApp.StepLexer.StepLexer _lexer = new();
        private readonly StepParser _parser = new();
        private readonly GrammarLoader _grammarLoader = new();
        private readonly Dictionary<string, RefactoringOperation> _refactoringOps = new();
        private GrammarDefinition? _currentGrammar;

        /// <summary>
        /// Current loaded grammar
        /// </summary>
        public GrammarDefinition? CurrentGrammar => _currentGrammar;

        /// <summary>
        /// Current parse context
        /// </summary>
        public ParseContext Context => _parser.Context;

        /// <summary>
        /// Load grammar from file
        /// </summary>
        public void LoadGrammar(string grammarFile)
        {
            _currentGrammar = _grammarLoader.LoadGrammar(grammarFile);
            ConfigureLexerAndParser();
            RegisterDefaultRefactoringOperations();
        }

        /// <summary>
        /// Load grammar from content string
        /// </summary>
        public void LoadGrammarFromContent(string grammarContent, string fileName = "inline")
        {
            _currentGrammar = _grammarLoader.ParseGrammarContent(grammarContent, fileName);
            ConfigureLexerAndParser();
            RegisterDefaultRefactoringOperations();
        }

        /// <summary>
        /// Configure lexer and parser with loaded grammar
        /// </summary>
        private void ConfigureLexerAndParser()
        {
            if (_currentGrammar == null) return;

            // Configure lexer with token rules
            foreach (var tokenRule in _currentGrammar.TokenRules)
            {
                _lexer.AddRule(tokenRule);
            }

            // Configure parser with production rules
            foreach (var productionRule in _currentGrammar.ProductionRules)
            {
                // Apply precedence and associativity
                if (_currentGrammar.Precedence.ContainsKey(productionRule.Name))
                {
                    productionRule.Precedence = _currentGrammar.Precedence[productionRule.Name];
                }
                
                _parser.AddRule(productionRule);
            }
        }

        /// <summary>
        /// Parse input text and return complete result
        /// </summary>
        public StepParsingResult Parse(string input, string fileName = "")
        {
            var startTime = DateTime.Now;
            var result = new StepParsingResult();

            try
            {
                // Convert string to UTF-8 bytes for zero-copy processing
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var inputMemory = new ReadOnlyMemory<byte>(inputBytes);

                // Phase 1: Lexical analysis
                _lexer.Initialize(inputMemory, fileName);
                var tokens = new List<StepToken>();

                // Safety limit to prevent infinite loops in lexer
                var maxLexerSteps = inputBytes.Length * 10; // Allow reasonable processing overhead
                var lexerStepCount = 0;
                var lastTokenCount = 0;

                while (!_lexer.ActivePaths.All(p => !p.IsValid || p.Position >= inputBytes.Length) && lexerStepCount < maxLexerSteps)
                {
                    var lexerResult = _lexer.Step();
                    tokens.AddRange(lexerResult.NewTokens);
                    lexerStepCount++;

                    if (lexerResult.IsComplete)
                        break;

                    // Progress check: if no new tokens after several steps, break to avoid infinite loop
                    if (lexerStepCount % 10 == 0 && tokens.Count == lastTokenCount)
                    {
                        result.Errors.Add($"Lexer appears stuck at step {lexerStepCount} with no progress");
                        break;
                    }
                    lastTokenCount = tokens.Count;
                }

                result.Tokens = tokens;

                // Phase 2: Syntactic analysis with GLR parsing
                _parser.Initialize(tokens, input);
                
                // Safety limit to prevent infinite loops in parser
                var maxParserSteps = tokens.Count * 20; // Allow reasonable processing overhead
                var parserStepCount = 0;
                var lastPosition = -1;
                var stuckCount = 0;

                while (parserStepCount < maxParserSteps)
                {
                    var parserResult = _parser.Step();
                    parserStepCount++;
                    
                    if (parserResult.IsComplete)
                    {
                        result.CognitiveGraph = _parser.SelectBestParseGraph();
                        result.AmbiguousParses = _parser.HandleAmbiguity();
                        break;
                    }

                    // Check for parsing failure
                    if (parserResult.ActivePathCount == 0)
                    {
                        result.Errors.Add($"Parse error at position {parserResult.CurrentPosition}");
                        break;
                    }

                    // Progress check: if position hasn't advanced in several steps, break to avoid infinite loop
                    if (parserResult.CurrentPosition == lastPosition)
                    {
                        stuckCount++;
                        if (stuckCount > 5)
                        {
                            result.Errors.Add($"Parser appears stuck at position {parserResult.CurrentPosition} after {parserStepCount} steps");
                            break;
                        }
                    }
                    else
                    {
                        stuckCount = 0;
                        lastPosition = parserResult.CurrentPosition;
                    }
                }

                result.Success = result.CognitiveGraph != null;
                result.PathCount = _parser.ActivePaths.Count;
                result.Context = _parser.Context;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Parsing exception: {ex.Message}");
            }

            result.ParseTime = DateTime.Now - startTime;
            return result;
        }

        /// <summary>
        /// Select code locations based on criteria (RefakTS-style)
        /// </summary>
        public List<ICodeLocation> Select(string file, SelectionCriteria criteria)
        {
            var locations = new List<ICodeLocation>();

            if (!File.Exists(file))
                return locations;

            var content = File.ReadAllText(file);
            var parseResult = Parse(content, file);

            if (!parseResult.Success || parseResult.CognitiveGraph == null)
                return locations;

            return SelectFromCognitiveGraph(parseResult.CognitiveGraph, criteria, file);
        }

        /// <summary>
        /// Select locations from CognitiveGraph based on criteria
        /// </summary>
        private List<ICodeLocation> SelectFromCognitiveGraph(CognitiveGraph.CognitiveGraph graph, SelectionCriteria criteria, string file)
        {
            var locations = new List<ICodeLocation>();
            var rootNode = graph.GetRootNode();
            
            return SelectFromSymbolNode(rootNode, criteria, file, graph);
        }

        /// <summary>
        /// Select locations from SymbolNode recursively
        /// </summary>
        private List<ICodeLocation> SelectFromSymbolNode(SymbolNode node, SelectionCriteria criteria, string file, CognitiveGraph.CognitiveGraph graph)
        {
            var locations = new List<ICodeLocation>();
            var sourceText = node.GetSourceText().ToString();

            // Regex-based selection
            if (!string.IsNullOrEmpty(criteria.Regex))
            {
                var regex = new System.Text.RegularExpressions.Regex(criteria.Regex);
                if (regex.IsMatch(sourceText))
                {
                    var location = new CodeLocation
                    {
                        File = file,
                        StartLine = 1, // Would need proper line calculation from byte position
                        StartColumn = (int)node.SourceStart,
                        EndLine = 1,
                        EndColumn = (int)node.SourceEnd,
                        Context = ""
                    };
                    locations.Add(location);
                }
            }

            // Structural selection
            if (criteria.Structural.HasValue)
            {
                var structural = criteria.Structural.Value;
                if (node.TryGetProperty("RuleName", out var ruleNameProp))
                {
                    var ruleName = ruleNameProp.AsString();
                    if (IsStructuralMatch(ruleName, structural.type))
                    {
                        var location = new CodeLocation
                        {
                            File = file,
                            StartLine = 1, // Would need proper line calculation from byte position
                            StartColumn = (int)node.SourceStart,
                            EndLine = 1,
                            EndColumn = (int)node.SourceEnd,
                            Context = ""
                        };
                        locations.Add(location);
                    }
                }
            }

            // Process packed nodes (ambiguous interpretations) - simplified for now
            if (node.IsAmbiguous)
            {
                // TODO: Implement proper packed node iteration once CognitiveGraph collection API is clarified
                // var packedNodes = node.GetPackedNodes();
            }

            return locations;
        }

        /// <summary>
        /// Check if rule name matches structural criteria
        /// </summary>
        private bool IsStructuralMatch(string ruleName, string type)
        {
            return ruleName.Equals(type, StringComparison.OrdinalIgnoreCase) ||
                   ruleName.Contains(type, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extract variable at location (RefakTS-style surgical operation)
        /// </summary>
        public RefactoringResult ExtractVariable(ICodeLocation location, string variableName)
        {
            var operation = _refactoringOps["extract-variable"];
            if (operation.Execute == null)
                return new RefactoringResult { Success = false, Message = "Extract variable operation not available" };

            // Find the node at the given location for context
            if (!TryFindNodeAtLocation(location, out var targetNode))
                return new RefactoringResult { Success = false, Message = "No parse node found at location" };

            return operation.Execute(location, _parser.Context);
        }

        /// <summary>
        /// Inline variable at location
        /// </summary>
        public RefactoringResult InlineVariable(ICodeLocation location)
        {
            var operation = _refactoringOps["inline-variable"];
            if (operation.Execute == null)
                return new RefactoringResult { Success = false, Message = "Inline variable operation not available" };

            if (!TryFindNodeAtLocation(location, out var targetNode))
                return new RefactoringResult { Success = false, Message = "No parse node found at location" };

            return operation.Execute(location, _parser.Context);
        }

        /// <summary>
        /// Rename symbol at location
        /// </summary>
        public RefactoringResult Rename(ICodeLocation location, string newName)
        {
            var operation = _refactoringOps["rename"];
            if (operation.Execute == null)
                return new RefactoringResult { Success = false, Message = "Rename operation not available" };

            if (!TryFindNodeAtLocation(location, out var targetNode))
                return new RefactoringResult { Success = false, Message = "No parse node found at location" };

            // Set the new name in context for the operation
            _parser.Context.Variables["newName"] = newName;

            return operation.Execute(location, _parser.Context);
        }

        /// <summary>
        /// Find all usages of symbol at location
        /// </summary>
        public List<ICodeLocation> FindUsages(ICodeLocation location, string scope = "")
        {
            if (!TryFindNodeAtLocation(location, out var targetNode))
                return new List<ICodeLocation>();

            // Get the symbol name from node properties
            var symbolName = "";
            if (targetNode.TryGetProperty("TokenValue", out var tokenValue))
            {
                symbolName = tokenValue.AsString();
            }

            var references = _parser.Context.SymbolTable.FindAllReferences(symbolName);

            if (!string.IsNullOrEmpty(scope))
            {
                return references.Where(r => r.Scope == scope).Select(r => r.Location).ToList();
            }

            return references.Select(r => r.Location).ToList();
        }

        /// <summary>
        /// Get applicable refactoring operations for location
        /// </summary>
        public List<RefactoringOperation> GetApplicableRefactorings(ICodeLocation location)
        {
            if (!TryFindNodeAtLocation(location, out var targetNode))
                return new List<RefactoringOperation>();

            var applicable = new List<RefactoringOperation>();
            var currentContext = _parser.Context.ContextStack.Current() ?? "";

            foreach (var operation in _refactoringOps.Values)
            {
                if (operation.ApplicableContexts.Length == 0 || 
                    operation.ApplicableContexts.Contains(currentContext))
                {
                    if (operation.Preconditions?.Invoke(_parser.Context) ?? true)
                    {
                        applicable.Add(operation);
                    }
                }
            }

            return applicable;
        }

        /// <summary>
        /// Find SymbolNode at specific location
        /// </summary>
        private bool TryFindNodeAtLocation(ICodeLocation location, out SymbolNode node)
        {
            // This would need to search through the current CognitiveGraph
            // For now, return false - in full implementation would traverse graph
            node = default;
            return false;
        }

        /// <summary>
        /// Register default refactoring operations (RefakTS-style)
        /// </summary>
        private void RegisterDefaultRefactoringOperations()
        {
            // Extract Variable
            _refactoringOps["extract-variable"] = new RefactoringOperation
            {
                Name = "extract-variable",
                Description = "Extract expression into a variable",
                ApplicableContexts = new[] { "function", "method", "block" },
                Preconditions = context => context.CurrentToken?.Type == "IDENTIFIER" || 
                                         context.CurrentToken?.Type == "expression",
                Execute = (location, context) =>
                {
                    var variableName = context.Variables.ContainsKey("variableName") 
                        ? context.Variables["variableName"].ToString() 
                        : "temp";
                    
                    return new RefactoringResult
                    {
                        Success = true,
                        Message = $"Extracted variable '{variableName}'",
                        Changes = new List<CodeChange>
                        {
                            new CodeChange
                            {
                                Location = location,
                                OriginalText = "expression", // Would need to extract from source
                                NewText = variableName ?? "temp",
                                ChangeType = "replace"
                            }
                        }
                    };
                }
            };

            // Inline Variable
            _refactoringOps["inline-variable"] = new RefactoringOperation
            {
                Name = "inline-variable",
                Description = "Inline variable usage with its value",
                ApplicableContexts = new[] { "function", "method", "block" },
                Execute = (location, context) =>
                {
                    // Would need to get symbol name from location - simplified for now
                    var symbolName = "variable"; // Placeholder
                    var symbolInfo = context.SymbolTable.Lookup(symbolName, 
                        context.ContextStack.Current() ?? "");
                    
                    if (symbolInfo?.CanInline == true && !string.IsNullOrEmpty(symbolInfo.Value))
                    {
                        return new RefactoringResult
                        {
                            Success = true,
                            Message = $"Inlined variable '{symbolName}'",
                            Changes = new List<CodeChange>
                            {
                                new CodeChange
                                {
                                    Location = location,
                                    OriginalText = symbolName,
                                    NewText = symbolInfo.Value,
                                    ChangeType = "replace"
                                }
                            }
                        };
                    }
                    
                    return new RefactoringResult 
                    { 
                        Success = false, 
                        Message = "Variable cannot be inlined" 
                    };
                }
            };

            // Rename
            _refactoringOps["rename"] = new RefactoringOperation
            {
                Name = "rename",
                Description = "Rename symbol and all its references",
                Execute = (location, context) =>
                {
                    var newName = context.Variables.ContainsKey("newName") 
                        ? context.Variables["newName"].ToString() 
                        : "renamed";
                    
                    // Would need to get symbol name from location - simplified for now
                    var symbolName = "symbol"; // Placeholder
                    var references = context.SymbolTable.FindAllReferences(symbolName);
                    var changes = references.Select(r => new CodeChange
                    {
                        Location = r.Location,
                        OriginalText = symbolName,
                        NewText = newName ?? "renamed",
                        ChangeType = "replace"
                    }).ToList();

                    return new RefactoringResult
                    {
                        Success = true,
                        Message = $"Renamed '{symbolName}' to '{newName}' ({references.Count()} references)",
                        Changes = changes
                    };
                }
            };
        }

        /// <summary>
        /// Register custom refactoring operation
        /// </summary>
        public void RegisterRefactoringOperation(RefactoringOperation operation)
        {
            _refactoringOps[operation.Name] = operation;
        }

        /// <summary>
        /// Execute projection match triggered code for semantic rules
        /// </summary>
        public void ExecuteProjection(string ruleName, string context, ICodeLocation location)
        {
            var projection = _grammarLoader.GetProjection(ruleName, context);
            if (projection?.ExecuteAction != null)
            {
                // Would need to get SymbolNode from location - simplified for now
                // projection.ExecuteAction(node, _parser.Context);
            }
        }

        /// <summary>
        /// Switch grammar context (for multi-language support)
        /// </summary>
        public void SwitchGrammarContext(string newContext)
        {
            _parser.Context.ContextStack.Push(newContext);
        }

        /// <summary>
        /// Get memory usage statistics (zero-copy architecture benefit)
        /// </summary>
        public (long bytesAllocated, int activeObjects) GetMemoryStats()
        {
            // In a full implementation, this would track actual memory usage
            // For demonstration, return estimated values
            var tokenCount = _parser.Context.Tokens.Count;
            var pathCount = _parser.ActivePaths.Count;
            
            return (tokenCount * 64 + pathCount * 128, tokenCount + pathCount);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _parser?.Dispose();
            // _lexer does not implement IDisposable currently
        }
    }
}