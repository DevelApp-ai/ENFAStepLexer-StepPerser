using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ENFA_Parser.Core
{
    /// <summary>
    /// Selection criteria for location-based targeting
    /// </summary>
    public class SelectionCriteria
    {
        public string? Regex { get; set; }
        public (string start, string end)? Range { get; set; }
        public (string type, bool includeFields, bool includeMethods)? Structural { get; set; }
        public string? Boundaries { get; set; }
        public string? Grammar { get; set; }
    }

    /// <summary>
    /// Refactoring operation definition
    /// </summary>
    public class RefactoringOperation
    {
        public string Name { get; set; } = string.Empty;
        public string[] ApplicableContexts { get; set; } = Array.Empty<string>();
        public Func<ParseContext, bool>? Preconditions { get; set; }
        public Func<ParseNode, ParseContext, RefactoringResult>? Execute { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of a refactoring operation
    /// </summary>
    public class RefactoringResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CodeChange> Changes { get; set; } = new();
        public ParseNode? ModifiedNode { get; set; }
    }

    /// <summary>
    /// Individual code change for surgical operations
    /// </summary>
    public class CodeChange
    {
        public ICodeLocation Location { get; set; } = new CodeLocation();
        public string OriginalText { get; set; } = string.Empty;
        public string NewText { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // insert, delete, replace
    }

    /// <summary>
    /// Complete step-parsing result
    /// </summary>
    public class StepParsingResult
    {
        public bool Success { get; set; }
        public ParseNode? ParseTree { get; set; }
        public List<ParseNode> AmbiguousParses { get; set; } = new();
        public List<StepToken> Tokens { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public TimeSpan ParseTime { get; set; }
        public int PathCount { get; set; }
        public ParseContext Context { get; set; } = new ParseContext();
    }

    /// <summary>
    /// Main step-parser engine coordinating lexer, parser, and semantic analysis
    /// Implements the GrammarForge architecture with location-based targeting and surgical operations
    /// </summary>
    public class StepParserEngine
    {
        private readonly StepLexer _lexer = new();
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

                while (!_lexer.ActivePaths.All(p => !p.IsValid || p.Position >= inputBytes.Length))
                {
                    var lexerResult = _lexer.Step();
                    tokens.AddRange(lexerResult.NewTokens);

                    if (lexerResult.IsComplete)
                        break;
                }

                result.Tokens = tokens;

                // Phase 2: Syntactic analysis with GLR parsing
                _parser.Initialize(tokens);
                
                while (true)
                {
                    var parserResult = _parser.Step();
                    
                    if (parserResult.IsComplete)
                    {
                        result.ParseTree = _parser.SelectBestParseTree();
                        result.AmbiguousParses = _parser.HandleAmbiguity();
                        break;
                    }

                    // Check for parsing failure
                    if (parserResult.ActivePathCount == 0)
                    {
                        result.Errors.Add($"Parse error at position {parserResult.CurrentPosition}");
                        break;
                    }
                }

                result.Success = result.ParseTree != null;
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

            if (!parseResult.Success || parseResult.ParseTree == null)
                return locations;

            return SelectFromParseTree(parseResult.ParseTree, criteria, file);
        }

        /// <summary>
        /// Select locations from parse tree based on criteria
        /// </summary>
        private List<ICodeLocation> SelectFromParseTree(ParseNode node, SelectionCriteria criteria, string file)
        {
            var locations = new List<ICodeLocation>();

            // Regex-based selection
            if (!string.IsNullOrEmpty(criteria.Regex))
            {
                var regex = new System.Text.RegularExpressions.Regex(criteria.Regex);
                if (regex.IsMatch(node.Value))
                {
                    locations.Add(node.Location);
                }
            }

            // Structural selection
            if (criteria.Structural.HasValue)
            {
                var structural = criteria.Structural.Value;
                if (IsStructuralMatch(node, structural.type))
                {
                    locations.Add(node.Location);
                }
            }

            // Range selection
            if (criteria.Range.HasValue)
            {
                var range = criteria.Range.Value;
                if (IsInRange(node, range.start, range.end))
                {
                    locations.Add(node.Location);
                }
            }

            // Recursively check children
            foreach (var child in node.Children)
            {
                locations.AddRange(SelectFromParseTree(child, criteria, file));
            }

            return locations;
        }

        /// <summary>
        /// Check if node matches structural criteria
        /// </summary>
        private bool IsStructuralMatch(ParseNode node, string type)
        {
            return node.RuleName.Equals(type, StringComparison.OrdinalIgnoreCase) ||
                   node.RuleName.Contains(type, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if node is within range
        /// </summary>
        private bool IsInRange(ParseNode node, string start, string end)
        {
            // Simplified range checking - in full implementation would use proper position comparison
            return node.Value.Contains(start) || node.Value.Contains(end);
        }

        /// <summary>
        /// Extract variable at location (RefakTS-style surgical operation)
        /// </summary>
        public RefactoringResult ExtractVariable(ICodeLocation location, string variableName)
        {
            var operation = _refactoringOps["extract-variable"];
            if (operation.Execute == null)
                return new RefactoringResult { Success = false, Message = "Extract variable operation not available" };

            // Find the parse node at the given location
            var targetNode = FindNodeAtLocation(location);
            if (targetNode == null)
                return new RefactoringResult { Success = false, Message = "No parse node found at location" };

            return operation.Execute(targetNode, _parser.Context);
        }

        /// <summary>
        /// Inline variable at location
        /// </summary>
        public RefactoringResult InlineVariable(ICodeLocation location)
        {
            var operation = _refactoringOps["inline-variable"];
            if (operation.Execute == null)
                return new RefactoringResult { Success = false, Message = "Inline variable operation not available" };

            var targetNode = FindNodeAtLocation(location);
            if (targetNode == null)
                return new RefactoringResult { Success = false, Message = "No parse node found at location" };

            return operation.Execute(targetNode, _parser.Context);
        }

        /// <summary>
        /// Rename symbol at location
        /// </summary>
        public RefactoringResult Rename(ICodeLocation location, string newName)
        {
            var operation = _refactoringOps["rename"];
            if (operation.Execute == null)
                return new RefactoringResult { Success = false, Message = "Rename operation not available" };

            var targetNode = FindNodeAtLocation(location);
            if (targetNode == null)
                return new RefactoringResult { Success = false, Message = "No parse node found at location" };

            // Set the new name in context for the operation
            _parser.Context.Variables["newName"] = newName;

            return operation.Execute(targetNode, _parser.Context);
        }

        /// <summary>
        /// Find all usages of symbol at location
        /// </summary>
        public List<ICodeLocation> FindUsages(ICodeLocation location, string scope = "")
        {
            var targetNode = FindNodeAtLocation(location);
            if (targetNode?.Token == null)
                return new List<ICodeLocation>();

            var symbolName = targetNode.Token.Value;
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
            var targetNode = FindNodeAtLocation(location);
            if (targetNode == null)
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
        /// Find parse node at specific location
        /// </summary>
        private ParseNode? FindNodeAtLocation(ICodeLocation location)
        {
            // This would need to search through the current parse tree
            // For now, return null - in full implementation would traverse parse tree
            return null;
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
                Execute = (node, context) =>
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
                                Location = node.Location,
                                OriginalText = node.Value,
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
                Execute = (node, context) =>
                {
                    var symbolInfo = context.SymbolTable.Lookup(node.Value, 
                        context.ContextStack.Current() ?? "");
                    
                    if (symbolInfo?.CanInline == true && !string.IsNullOrEmpty(symbolInfo.Value))
                    {
                        return new RefactoringResult
                        {
                            Success = true,
                            Message = $"Inlined variable '{node.Value}'",
                            Changes = new List<CodeChange>
                            {
                                new CodeChange
                                {
                                    Location = node.Location,
                                    OriginalText = node.Value,
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
                Execute = (node, context) =>
                {
                    var newName = context.Variables.ContainsKey("newName") 
                        ? context.Variables["newName"].ToString() 
                        : "renamed";
                    
                    var references = context.SymbolTable.FindAllReferences(node.Value);
                    var changes = references.Select(r => new CodeChange
                    {
                        Location = r.Location,
                        OriginalText = node.Value,
                        NewText = newName ?? "renamed",
                        ChangeType = "replace"
                    }).ToList();

                    return new RefactoringResult
                    {
                        Success = true,
                        Message = $"Renamed '{node.Value}' to '{newName}' ({references.Length} references)",
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
        public void ExecuteProjection(string ruleName, string context, ParseNode node)
        {
            var projection = _grammarLoader.GetProjection(ruleName, context);
            if (projection?.ExecuteAction != null)
            {
                projection.ExecuteAction(node, _parser.Context);
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
    }
}