using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DevelApp.StepLexer;
using CognitiveGraph.Accessors;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Grammar definition loaded from file
    /// </summary>
    public class GrammarDefinition
    {
        /// <summary>Gets or sets the name of the grammar definition.</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the token splitter strategy (default: "Space").</summary>
        public string TokenSplitter { get; set; } = "Space";
        
        /// <summary>Gets or sets the list of token rules for lexical analysis.</summary>
        public List<TokenRule> TokenRules { get; set; } = new();
        
        /// <summary>Gets or sets the list of production rules for parsing.</summary>
        public List<ProductionRule> ProductionRules { get; set; } = new();
        
        /// <summary>Gets or sets the precedence values for operators and productions.</summary>
        public Dictionary<string, int> Precedence { get; set; } = new();
        
        /// <summary>Gets or sets the associativity rules (left, right, none) for operators.</summary>
        public Dictionary<string, string> Associativity { get; set; } = new();
        
        /// <summary>Gets or sets the list of available parsing contexts.</summary>
        public List<string> Contexts { get; set; } = new();
        
        /// <summary>Gets or sets the semantic actions triggered during parsing.</summary>
        public Dictionary<string, Action<GraphNodeRef, List<GraphNodeRef>, CognitiveGraph.Builder.CognitiveGraphBuilder>> SemanticActions { get; set; } = new();
        
        /// <summary>Gets or sets the list of imported grammar files.</summary>
        public List<string> Imports { get; set; } = new();
        
        /// <summary>Gets or sets whether this grammar can be inherited by other grammars.</summary>
        public bool IsInheritable { get; set; }
        
        /// <summary>Gets or sets the format type of the grammar (ANTLR, Bison, etc.).</summary>
        public string FormatType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Context-sensitive projection for semantic rules
    /// </summary>
    public class ContextProjection
    {
        /// <summary>Gets or sets the name of the production rule this projection applies to.</summary>
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the context in which this projection is active.</summary>
        public string Context { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the projection pattern that triggers this rule.</summary>
        public string ProjectionPattern { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the code triggered when this projection matches.</summary>
        public string TriggeredCode { get; set; } = string.Empty;
        
        /// <summary>Gets or sets the parameters for the projection.</summary>
        public List<string> Parameters { get; set; } = new();
        
        /// <summary>Gets or sets the condition function that determines if the projection matches.</summary>
        public Func<ParseContext, bool>? MatchCondition { get; set; }
        
        /// <summary>Gets or sets the action to execute when the projection is triggered.</summary>
        public Action<ICodeLocation, ParseContext>? ExecuteAction { get; set; }

        /// <summary>
        /// Initializes a new instance of the ContextProjection class.
        /// </summary>
        /// <param name="ruleName">The name of the production rule</param>
        /// <param name="context">The context in which this projection is active</param>
        /// <param name="pattern">The projection pattern that triggers this rule</param>
        /// <param name="code">The code triggered when this projection matches</param>
        public ContextProjection(string ruleName, string context, string pattern, string code)
        {
            RuleName = ruleName;
            Context = context;
            ProjectionPattern = pattern;
            TriggeredCode = code;
        }
    }

    /// <summary>
    /// Loader for grammar files with support for context-sensitive projections
    /// and projection match triggered code for semantic rules
    /// </summary>
    public class GrammarLoader
    {
        private readonly Dictionary<string, GrammarDefinition> _loadedGrammars = new();
        private readonly Dictionary<string, ContextProjection> _projections = new();

        /// <summary>
        /// Load grammar from file
        /// </summary>
        public GrammarDefinition LoadGrammar(string filePath)
        {
            if (_loadedGrammars.TryGetValue(filePath, out var cached))
                return cached;

            var content = File.ReadAllText(filePath);
            var grammar = ParseGrammarContent(content, filePath);
            
            _loadedGrammars[filePath] = grammar;
            return grammar;
        }

        /// <summary>
        /// Parse grammar content from string
        /// </summary>
        public GrammarDefinition ParseGrammarContent(string content, string fileName = "")
        {
            var grammar = new GrammarDefinition();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            ParseGrammarHeader(lines, grammar);
            ParseTokenRules(lines, grammar);
            ParseProductionRules(lines, grammar);
            ParsePrecedenceRules(lines, grammar);
            ParseContextProjections(lines, grammar);
            ProcessInheritance(grammar);
            
            return grammar;
        }

        /// <summary>
        /// Parse grammar header information
        /// </summary>
        private void ParseGrammarHeader(string[] lines, GrammarDefinition grammar)
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Grammar:"))
                {
                    grammar.Name = trimmed.Substring(8).Trim();
                }
                else if (trimmed.StartsWith("TokenSplitter:"))
                {
                    grammar.TokenSplitter = trimmed.Substring(14).Trim();
                }
                else if (trimmed.StartsWith("Inherits:"))
                {
                    var inherits = trimmed.Substring(9).Trim();
                    grammar.Imports.AddRange(inherits.Split(',').Select(s => s.Trim()));
                }
                else if (trimmed.StartsWith("Inheritable:"))
                {
                    grammar.IsInheritable = bool.Parse(trimmed.Substring(12).Trim());
                }
                else if (trimmed.StartsWith("FormatType:"))
                {
                    grammar.FormatType = trimmed.Substring(11).Trim();
                }
            }
        }

        /// <summary>
        /// Parse token rules from grammar content
        /// </summary>
        private void ParseTokenRules(string[] lines, GrammarDefinition grammar)
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (IsTokenRule(trimmed))
                {
                    var tokenRule = ParseTokenRule(trimmed);
                    if (tokenRule != null)
                    {
                        grammar.TokenRules.Add(tokenRule);
                    }
                }
            }
        }

        /// <summary>
        /// Check if line defines a token rule
        /// </summary>
        private bool IsTokenRule(string line)
        {
            // Token rules start with < and contain > followed by ::= 
            // and have patterns like /regex/, 'literal', or "string" on the right side
            if (!line.StartsWith("<") || !line.Contains("::=")) return false;
            
            var colonIndex = line.IndexOf("::=");
            if (colonIndex == -1) return false;
            
            var rightSide = line.Substring(colonIndex + 3).Trim();
            
            // Token rules have patterns, not references to other rules
            return rightSide.StartsWith("/") || rightSide.StartsWith("'") || rightSide.StartsWith("\"");
        }

        /// <summary>
        /// Parse individual token rule
        /// </summary>
        private TokenRule? ParseTokenRule(string line)
        {
            try
            {
                // Pattern: <TOKEN_NAME> ::= pattern => { action }
                // Fixed regex to properly capture the full pattern
                var match = Regex.Match(line, @"<([^>]+)>\s*::=\s*(.+?)(?:\s*=>\s*\{([^}]*)\})?$");
                if (!match.Success) return null;

                var name = match.Groups[1].Value.Trim();
                var pattern = match.Groups[2].Value.Trim();
                var action = match.Groups[3].Success ? match.Groups[3].Value.Trim() : "";

                var context = ExtractContext(name);
                var cleanName = RemoveContext(name);

                var rule = new TokenRule(cleanName, pattern, context.context)
                {
                    Priority = context.priority,
                    IsSkippable = action.Contains("skip") || action.Contains("/* skip")
                };

                if (!string.IsNullOrEmpty(action))
                {
                    rule.Action = CreateTokenAction(action);
                }

                return rule;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing token rule: {line}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse production rules from grammar content
        /// </summary>
        private void ParseProductionRules(string[] lines, GrammarDefinition grammar)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (IsProductionRule(line))
                {
                    var fullRule = CollectMultiLineRule(lines, ref i);
                    var productionRule = ParseProductionRule(fullRule);
                    if (productionRule != null)
                    {
                        grammar.ProductionRules.Add(productionRule);
                    }
                }
            }
        }

        /// <summary>
        /// Check if line defines a production rule
        /// </summary>
        private bool IsProductionRule(string line)
        {
            // Production rules start with < and contain ::= but are not token rules
            return line.StartsWith("<") && line.Contains("::=") && !IsTokenRule(line);
        }

        /// <summary>
        /// Collect multi-line rule definition
        /// </summary>
        private string CollectMultiLineRule(string[] lines, ref int startIndex)
        {
            var ruleText = lines[startIndex];
            
            // Continue collecting lines until we find a complete rule or hit another rule
            while (startIndex + 1 < lines.Length && 
                   !lines[startIndex + 1].Trim().StartsWith("<") &&
                   !ruleText.Contains("=>") ||
                   (ruleText.Count(c => c == '{') > ruleText.Count(c => c == '}')))
            {
                startIndex++;
                ruleText += " " + lines[startIndex].Trim();
            }

            return ruleText;
        }

        /// <summary>
        /// Parse individual production rule
        /// </summary>
        private ProductionRule? ParseProductionRule(string ruleText)
        {
            try
            {
                // Pattern: <rule_name (context)> ::= rhs | rhs => { action }
                var match = Regex.Match(ruleText, @"<([^>]+)>\s*::=\s*(.+?)(?:\s*=>\s*\{([^}]*)\})?$", RegexOptions.Singleline);
                if (!match.Success) return null;

                var nameWithContext = match.Groups[1].Value.Trim();
                var rhsText = match.Groups[2].Value.Trim();
                var action = match.Groups[3].Success ? match.Groups[3].Value.Trim() : "";

                var context = ExtractContext(nameWithContext);
                var cleanName = RemoveContext(nameWithContext);

                // Handle alternatives (|)
                var alternatives = rhsText.Split('|');
                var firstAlternative = alternatives[0].Trim();
                
                // Parse RHS symbols
                var rhs = ParseRightHandSide(firstAlternative);
                
                var rule = new ProductionRule(cleanName, rhs, context.context)
                {
                    Precedence = context.priority
                };

                if (!string.IsNullOrEmpty(action))
                {
                    rule.SemanticAction = CreateSemanticAction(action, cleanName);
                }

                return rule;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing production rule: {ruleText}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse right-hand side of production rule
        /// </summary>
        private List<string> ParseRightHandSide(string rhs)
        {
            var symbols = new List<string>();
            var matches = Regex.Matches(rhs, @"<([^>]+)>|""([^""]+)""|'([^']+)'|([a-zA-Z_][a-zA-Z0-9_]*)");
            
            foreach (Match match in matches)
            {
                if (match.Groups[1].Success) // <non-terminal>
                {
                    symbols.Add(match.Groups[1].Value);
                }
                else if (match.Groups[2].Success) // "terminal"
                {
                    symbols.Add($"\"{match.Groups[2].Value}\"");
                }
                else if (match.Groups[3].Success) // 'terminal'
                {
                    symbols.Add($"'{match.Groups[3].Value}'");
                }
                else if (match.Groups[4].Success) // TERMINAL
                {
                    symbols.Add(match.Groups[4].Value);
                }
            }

            return symbols;
        }

        /// <summary>
        /// Extract context information from rule name
        /// </summary>
        private (string context, int priority) ExtractContext(string nameWithContext)
        {
            var match = Regex.Match(nameWithContext, @"([^(]+)(?:\(([^)]+)\))?");
            if (!match.Success) return ("", 0);

            var name = match.Groups[1].Value.Trim();
            var context = match.Groups[2].Success ? match.Groups[2].Value.Trim() : "";
            
            // Extract priority if specified
            var priorityMatch = Regex.Match(context, @"priority:(\d+)");
            var priority = priorityMatch.Success ? int.Parse(priorityMatch.Groups[1].Value) : 0;
            
            // Remove priority specification from context
            context = Regex.Replace(context, @",?\s*priority:\d+", "").Trim();

            return (context, priority);
        }

        /// <summary>
        /// Remove context modifiers from name
        /// </summary>
        private string RemoveContext(string nameWithContext)
        {
            var parenIndex = nameWithContext.IndexOf('(');
            return parenIndex > 0 ? nameWithContext.Substring(0, parenIndex).Trim() : nameWithContext.Trim();
        }

        /// <summary>
        /// Parse precedence and associativity rules
        /// </summary>
        private void ParsePrecedenceRules(string[] lines, GrammarDefinition grammar)
        {
            bool inPrecedenceBlock = false;
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                if (trimmed.StartsWith("Precedence:"))
                {
                    inPrecedenceBlock = true;
                    continue;
                }
                
                if (inPrecedenceBlock)
                {
                    if (trimmed.StartsWith("}") || string.IsNullOrEmpty(trimmed))
                    {
                        inPrecedenceBlock = false;
                        continue;
                    }
                    
                    ParsePrecedenceLevel(trimmed, grammar);
                }
            }
        }

        /// <summary>
        /// Parse individual precedence level
        /// </summary>
        private void ParsePrecedenceLevel(string line, GrammarDefinition grammar)
        {
            // Format: Level1: { operators: ["*", "/"], associativity: "left" }
            var match = Regex.Match(line, @"Level(\d+):\s*\{\s*operators:\s*\[([^\]]+)\],\s*associativity:\s*""([^""]+)""\s*\}");
            if (match.Success)
            {
                var level = int.Parse(match.Groups[1].Value);
                var operators = match.Groups[2].Value.Split(',')
                    .Select(op => op.Trim().Trim('"'))
                    .ToArray();
                var associativity = match.Groups[3].Value;

                foreach (var op in operators)
                {
                    grammar.Precedence[op] = level;
                    grammar.Associativity[op] = associativity;
                }
            }
        }

        /// <summary>
        /// Parse context-sensitive projections
        /// </summary>
        private void ParseContextProjections(string[] lines, GrammarDefinition grammar)
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (IsContextProjection(trimmed))
                {
                    var projection = ParseContextProjection(trimmed);
                    if (projection != null)
                    {
                        _projections[projection.RuleName + ":" + projection.Context] = projection;
                    }
                }
            }
        }

        /// <summary>
        /// Check if line defines a context projection
        /// </summary>
        private bool IsContextProjection(string line)
        {
            // Context projections have specific patterns for triggered code
            return line.Contains("@context") || line.Contains("@projection");
        }

        /// <summary>
        /// Parse context projection definition
        /// </summary>
        private ContextProjection? ParseContextProjection(string line)
        {
            try
            {
                // Pattern: @context(context_name) @projection(pattern) rule_name => { code }
                var match = Regex.Match(line, @"@context\(([^)]+)\)\s*@projection\(([^)]+)\)\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*=>\s*\{([^}]*)\}");
                if (!match.Success) return null;

                var context = match.Groups[1].Value.Trim();
                var pattern = match.Groups[2].Value.Trim();
                var ruleName = match.Groups[3].Value.Trim();
                var code = match.Groups[4].Value.Trim();

                var projection = new ContextProjection(ruleName, context, pattern, code);
                projection.ExecuteAction = CreateProjectionAction(code, ruleName, context);

                return projection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing context projection: {line}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Process grammar inheritance
        /// </summary>
        private void ProcessInheritance(GrammarDefinition grammar)
        {
            foreach (var import in grammar.Imports)
            {
                var baseGrammar = LoadBaseGrammar(import);
                if (baseGrammar != null)
                {
                    MergeGrammars(grammar, baseGrammar);
                }
            }
        }

        /// <summary>
        /// Load base grammar for inheritance
        /// </summary>
        private GrammarDefinition? LoadBaseGrammar(string baseName)
        {
            // In a full implementation, this would resolve base grammar files
            // For now, return a simplified base grammar
            return CreateDefaultBaseGrammar(baseName);
        }

        /// <summary>
        /// Create default base grammars for common parser types
        /// </summary>
        private GrammarDefinition? CreateDefaultBaseGrammar(string baseName)
        {
            var baseGrammar = new GrammarDefinition { Name = baseName };

            switch (baseName.ToLower())
            {
                case "antlr4_base":
                    // Add common ANTLR v4 patterns
                    baseGrammar.TokenRules.Add(new TokenRule("WS", "/[ \\t\\r\\n]+/", "", 0) { IsSkippable = true });
                    baseGrammar.TokenRules.Add(new TokenRule("IDENTIFIER", "/[a-zA-Z][a-zA-Z0-9]*/"));
                    baseGrammar.TokenRules.Add(new TokenRule("NUMBER", "/[0-9]+/"));
                    break;

                case "bison_base":
                    // Add common Bison patterns
                    baseGrammar.Precedence["+"] = 1;
                    baseGrammar.Precedence["-"] = 1;
                    baseGrammar.Precedence["*"] = 2;
                    baseGrammar.Precedence["/"] = 2;
                    baseGrammar.Associativity["+"] = "left";
                    baseGrammar.Associativity["-"] = "left";
                    baseGrammar.Associativity["*"] = "left";
                    baseGrammar.Associativity["/"] = "left";
                    break;
            }

            return baseGrammar;
        }

        /// <summary>
        /// Merge base grammar into derived grammar
        /// </summary>
        private void MergeGrammars(GrammarDefinition derived, GrammarDefinition baseGrammar)
        {
            // Merge token rules (base rules first, then derived overrides)
            var mergedTokens = new Dictionary<string, TokenRule>();
            
            foreach (var rule in baseGrammar.TokenRules)
            {
                mergedTokens[rule.Name] = rule;
            }
            
            foreach (var rule in derived.TokenRules)
            {
                mergedTokens[rule.Name] = rule; // Override base rules
            }
            
            derived.TokenRules = mergedTokens.Values.ToList();

            // Merge precedence rules
            foreach (var kvp in baseGrammar.Precedence)
            {
                if (!derived.Precedence.ContainsKey(kvp.Key))
                {
                    derived.Precedence[kvp.Key] = kvp.Value;
                }
            }

            // Merge associativity rules
            foreach (var kvp in baseGrammar.Associativity)
            {
                if (!derived.Associativity.ContainsKey(kvp.Key))
                {
                    derived.Associativity[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Create token action from code string
        /// </summary>
        private Action<StepToken> CreateTokenAction(string code)
        {
            return token =>
            {
                // Simplified action execution - in real implementation would use proper code execution
                if (code.Contains("skip"))
                {
                    // Mark token as skippable
                }
                else if (code.Contains("return"))
                {
                    // Process return statement
                    var match = Regex.Match(code, @"return\s*\(\s*""([^""]+)""\s*\)");
                    if (match.Success)
                    {
                        token.Type = match.Groups[1].Value;
                    }
                }
            };
        }

        /// <summary>
        /// Create semantic action from code string
        /// </summary>
        private Action<GraphNodeRef, List<GraphNodeRef>, CognitiveGraph.Builder.CognitiveGraphBuilder> CreateSemanticAction(string code, string ruleName)
        {
            return (node, children, builder) =>
            {
                // Simplified semantic action execution
                if (code.Contains("createBinaryOp"))
                {
                    // Would create additional properties or nodes using builder
                    // node.Value = $"BinaryOp({children[0].Value}, {children[1].Value}, {children[2].Value})";
                }
                else if (code.Contains("$2"))
                {
                    // Simple parameter substitution
                    if (children.Count > 1)
                        node.Value = children[1].Value;
                }
            };
        }

        /// <summary>
        /// Create projection action from code string
        /// </summary>
        private Action<ICodeLocation, ParseContext> CreateProjectionAction(string code, string ruleName, string context)
        {
            return (node, parseContext) =>
            {
                // Execute projection-triggered code based on context
                Console.WriteLine($"Executing projection for {ruleName} in context {context}: {code}");
                
                // In a full implementation, this would execute the actual code
                // For now, just log the execution
            };
        }

        /// <summary>
        /// Get context projection for rule and context
        /// </summary>
        public ContextProjection? GetProjection(string ruleName, string context)
        {
            return _projections.TryGetValue(ruleName + ":" + context, out var projection) ? projection : null;
        }
    }
}