using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENFA_Parser.Core
{
    /// <summary>
    /// Token produced by the step lexer with position information
    /// </summary>
    public class StepToken
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ICodeLocation Location { get; set; } = new CodeLocation();
        public string Context { get; set; } = string.Empty;
        public bool IsSplittable { get; set; }
        public List<StepToken>? SplitTokens { get; set; }

        public StepToken() { }

        public StepToken(string type, string value, ICodeLocation location, string context = "")
        {
            Type = type;
            Value = value;
            Location = location;
            Context = context;
        }

        public override string ToString()
        {
            return $"{Type}:'{Value}' @{Location}";
        }
    }

    /// <summary>
    /// Lexer path for handling multiple tokenization possibilities
    /// </summary>
    public class LexerPath
    {
        public int PathId { get; set; }
        public int Position { get; set; }
        public List<StepToken> Tokens { get; set; } = new();
        public string CurrentContext { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public Dictionary<string, object> State { get; set; } = new();

        public LexerPath(int pathId, int position = 0)
        {
            PathId = pathId;
            Position = position;
        }

        public LexerPath Clone(int newPathId)
        {
            return new LexerPath(newPathId, Position)
            {
                Tokens = new List<StepToken>(Tokens),
                CurrentContext = CurrentContext,
                IsValid = IsValid,
                State = new Dictionary<string, object>(State)
            };
        }
    }

    /// <summary>
    /// Grammar rule for tokenization
    /// </summary>
    public class TokenRule
    {
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public bool IsFragment { get; set; }
        public bool IsSkippable { get; set; }
        public Action<StepToken>? Action { get; set; }

        public TokenRule(string name, string pattern, string context = "", int priority = 0)
        {
            Name = name;
            Pattern = pattern;
            Context = context;
            Priority = priority;
        }
    }

    /// <summary>
    /// Step-by-step lexer with multiple path support for ambiguous tokenization
    /// </summary>
    public class StepLexer
    {
        private readonly List<TokenRule> _rules = new();
        private readonly List<LexerPath> _activePaths = new();
        private readonly IContextStack _contextStack = new ContextStack();
        private ReadOnlyMemory<byte> _input;
        private string _fileName = string.Empty;
        private int _nextPathId = 0;

        /// <summary>
        /// Current active paths being tracked
        /// </summary>
        public IReadOnlyList<LexerPath> ActivePaths => _activePaths;

        /// <summary>
        /// Current context stack
        /// </summary>
        public IContextStack ContextStack => _contextStack;

        /// <summary>
        /// Add a tokenization rule
        /// </summary>
        public void AddRule(TokenRule rule)
        {
            _rules.Add(rule);
            // Sort by priority (higher priority first)
            _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Initialize lexer with input text
        /// </summary>
        public void Initialize(ReadOnlyMemory<byte> input, string fileName = "")
        {
            _input = input;
            _fileName = fileName;
            _activePaths.Clear();
            _activePaths.Add(new LexerPath(_nextPathId++, 0));
            _contextStack.Push("default");
        }

        /// <summary>
        /// Process next character and return updated paths
        /// </summary>
        public LexerStepResult Step()
        {
            var result = new LexerStepResult();
            var newPaths = new List<LexerPath>();

            foreach (var path in _activePaths.Where(p => p.IsValid))
            {
                var stepResult = ProcessPath(path);
                result.NewTokens.AddRange(stepResult.Tokens);
                newPaths.AddRange(stepResult.Paths);
                
                if (stepResult.ContextChanges.Any())
                {
                    result.ContextChanges.AddRange(stepResult.ContextChanges);
                }
            }

            // Update active paths and merge identical paths
            _activePaths.Clear();
            _activePaths.AddRange(MergePaths(newPaths));

            // Remove invalid paths
            _activePaths.RemoveAll(p => !p.IsValid);

            result.ActivePathCount = _activePaths.Count;
            result.IsComplete = _activePaths.All(p => p.Position >= _input.Length);

            return result;
        }

        /// <summary>
        /// Process a single lexer path
        /// </summary>
        private PathStepResult ProcessPath(LexerPath path)
        {
            var result = new PathStepResult();

            if (path.Position >= _input.Length)
            {
                path.IsValid = false;
                return result;
            }

            var remainingInput = _input.Span.Slice(path.Position);
            var (line, column) = CalculateLineColumn(path.Position);
            
            // Try to match rules in priority order
            var matches = new List<(TokenRule rule, int length, string matchText)>();

            foreach (var rule in _rules)
            {
                // Check context compatibility
                if (!IsRuleApplicableInContext(rule, path.CurrentContext))
                    continue;

                var match = TryMatchRule(rule, remainingInput);
                if (match.success)
                {
                    matches.Add((rule, match.length, match.text));
                }
            }

            if (matches.Count == 0)
            {
                // No matches - invalid path
                path.IsValid = false;
                return result;
            }

            // Handle multiple matches - create paths for ambiguity
            if (matches.Count == 1)
            {
                var (rule, length, text) = matches[0];
                ProcessSingleMatch(path, rule, text, length, line, column, result);
            }
            else
            {
                // Multiple matches - create split paths
                ProcessMultipleMatches(path, matches, line, column, result);
            }

            return result;
        }

        /// <summary>
        /// Process a single rule match
        /// </summary>
        private void ProcessSingleMatch(LexerPath path, TokenRule rule, string matchText, int length, 
            int line, int column, PathStepResult result)
        {
            path.Position += length;

            if (!rule.IsSkippable)
            {
                var location = new CodeLocation(_fileName, line, column, line, column + length, path.CurrentContext);
                var token = new StepToken(rule.Name, matchText, location, path.CurrentContext);
                
                // Check if token can be split for ambiguity resolution
                if (CanSplitToken(token, rule))
                {
                    token.IsSplittable = true;
                    token.SplitTokens = GenerateSplitTokens(token, rule, location);
                }

                path.Tokens.Add(token);
                result.Tokens.Add(token);

                // Execute rule action if present
                rule.Action?.Invoke(token);
            }

            result.Paths.Add(path);
        }

        /// <summary>
        /// Process multiple rule matches by creating split paths
        /// </summary>
        private void ProcessMultipleMatches(LexerPath originalPath, 
            List<(TokenRule rule, int length, string text)> matches, 
            int line, int column, PathStepResult result)
        {
            foreach (var (rule, length, text) in matches)
            {
                var newPath = originalPath.Clone(_nextPathId++);
                ProcessSingleMatch(newPath, rule, text, length, line, column, result);
            }
        }

        /// <summary>
        /// Check if a rule is applicable in the current context
        /// </summary>
        private bool IsRuleApplicableInContext(TokenRule rule, string currentContext)
        {
            if (string.IsNullOrEmpty(rule.Context))
                return true; // Rule applies to all contexts

            return rule.Context == currentContext || _contextStack.Contains(rule.Context);
        }

        /// <summary>
        /// Try to match a rule pattern against input
        /// </summary>
        private (bool success, int length, string text) TryMatchRule(TokenRule rule, ReadOnlySpan<byte> input)
        {
            // Simplified pattern matching - in a real implementation, this would use proper regex/pattern engines
            var pattern = rule.Pattern;
            
            if (pattern.StartsWith("/") && pattern.EndsWith("/"))
            {
                // Regex pattern - simplified implementation
                var regex = pattern[1..^1];
                return TryMatchRegex(regex, input);
            }
            else if (pattern.StartsWith("\"") && pattern.EndsWith("\""))
            {
                // Literal string match
                var literal = pattern[1..^1];
                return TryMatchLiteral(literal, input);
            }
            
            // Default to literal match
            return TryMatchLiteral(pattern, input);
        }

        /// <summary>
        /// Simple regex matching (placeholder - would use proper regex in real implementation)
        /// </summary>
        private (bool success, int length, string text) TryMatchRegex(string pattern, ReadOnlySpan<byte> input)
        {
            // Placeholder implementation for common patterns
            switch (pattern)
            {
                case "[0-9]+":
                    return MatchDigits(input);
                case "[a-zA-Z][a-zA-Z0-9]*":
                    return MatchIdentifier(input);
                case "[ \\t\\r\\n]+":
                    return MatchWhitespace(input);
                default:
                    return (false, 0, string.Empty);
            }
        }

        /// <summary>
        /// Match literal string
        /// </summary>
        private (bool success, int length, string text) TryMatchLiteral(string literal, ReadOnlySpan<byte> input)
        {
            var literalBytes = Encoding.UTF8.GetBytes(literal);
            if (input.Length >= literalBytes.Length && input.StartsWith(literalBytes))
            {
                return (true, literalBytes.Length, literal);
            }
            return (false, 0, string.Empty);
        }

        /// <summary>
        /// Match digits pattern
        /// </summary>
        private (bool success, int length, string text) MatchDigits(ReadOnlySpan<byte> input)
        {
            int length = 0;
            while (length < input.Length && input[length] >= '0' && input[length] <= '9')
            {
                length++;
            }
            
            if (length > 0)
            {
                var text = Encoding.UTF8.GetString(input.Slice(0, length));
                return (true, length, text);
            }
            return (false, 0, string.Empty);
        }

        /// <summary>
        /// Match identifier pattern
        /// </summary>
        private (bool success, int length, string text) MatchIdentifier(ReadOnlySpan<byte> input)
        {
            if (input.Length == 0) return (false, 0, string.Empty);
            
            int length = 0;
            var first = input[0];
            
            // First character must be letter
            if (!((first >= 'a' && first <= 'z') || (first >= 'A' && first <= 'Z')))
                return (false, 0, string.Empty);
            
            length = 1;
            
            // Subsequent characters can be letters or digits
            while (length < input.Length)
            {
                var ch = input[length];
                if (!((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9')))
                    break;
                length++;
            }
            
            var text = Encoding.UTF8.GetString(input.Slice(0, length));
            return (true, length, text);
        }

        /// <summary>
        /// Match whitespace pattern
        /// </summary>
        private (bool success, int length, string text) MatchWhitespace(ReadOnlySpan<byte> input)
        {
            int length = 0;
            while (length < input.Length)
            {
                var ch = input[length];
                if (!(ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n'))
                    break;
                length++;
            }
            
            if (length > 0)
            {
                var text = Encoding.UTF8.GetString(input.Slice(0, length));
                return (true, length, text);
            }
            return (false, 0, string.Empty);
        }

        /// <summary>
        /// Check if token can be split for ambiguity resolution
        /// </summary>
        private bool CanSplitToken(StepToken token, TokenRule rule)
        {
            // For demonstration - tokens like "\x{41}\xFF" can be split
            return token.Value.Contains("\\x{") && token.Value.Contains("}");
        }

        /// <summary>
        /// Generate split tokens for ambiguity resolution
        /// </summary>
        private List<StepToken> GenerateSplitTokens(StepToken originalToken, TokenRule rule, ICodeLocation location)
        {
            var splits = new List<StepToken>();
            
            // Example implementation for hex escape splitting
            if (originalToken.Value.Contains("\\x{") && originalToken.Value.Contains("}"))
            {
                // Split into individual hex escapes
                var parts = originalToken.Value.Split(new[] { "\\x{" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (part.Contains("}"))
                    {
                        var hex = part.Substring(0, part.IndexOf("}"));
                        splits.Add(new StepToken("HEX_ESCAPE", $"\\x{{{hex}}}", location, originalToken.Context));
                    }
                }
            }
            
            return splits;
        }

        /// <summary>
        /// Merge identical paths to optimize performance
        /// </summary>
        private List<LexerPath> MergePaths(List<LexerPath> paths)
        {
            var merged = new Dictionary<string, LexerPath>();
            
            foreach (var path in paths)
            {
                var key = $"{path.Position}:{path.CurrentContext}:{string.Join(",", path.Tokens.Select(t => t.Type))}";
                if (!merged.ContainsKey(key))
                {
                    merged[key] = path;
                }
            }
            
            return merged.Values.ToList();
        }

        /// <summary>
        /// Calculate line and column from byte position
        /// </summary>
        private (int line, int column) CalculateLineColumn(int position)
        {
            int line = 1, column = 1;
            
            for (int i = 0; i < Math.Min(position, _input.Length); i++)
            {
                if (_input.Span[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
            
            return (line, column);
        }
    }

    /// <summary>
    /// Result of a single lexer step
    /// </summary>
    public class LexerStepResult
    {
        public List<StepToken> NewTokens { get; set; } = new();
        public List<string> ContextChanges { get; set; } = new();
        public int ActivePathCount { get; set; }
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Result of processing a single path
    /// </summary>
    public class PathStepResult
    {
        public List<StepToken> Tokens { get; set; } = new();
        public List<LexerPath> Paths { get; set; } = new();
        public List<string> ContextChanges { get; set; } = new();
    }
}