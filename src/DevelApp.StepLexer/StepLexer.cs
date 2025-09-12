using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Represents a token that can be split into multiple alternatives during ambiguity resolution
    /// Used for both regex pattern parsing and source code tokenization
    /// </summary>
    public class SplittableToken
    {
        /// <summary>
        /// Gets the text content of the token as a zero-copy string view
        /// </summary>
        public ZeroCopyStringView Text { get; }
        
        /// <summary>
        /// Gets or sets the type of the token
        /// </summary>
        public TokenType Type { get; set; }
        
        /// <summary>
        /// Gets the position of the token in the input
        /// </summary>
        public int Position { get; }
        
        /// <summary>
        /// Gets or sets the list of alternative tokens when ambiguity is detected
        /// </summary>
        public List<SplittableToken>? Alternatives { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the SplittableToken class
        /// </summary>
        /// <param name="text">The text content of the token</param>
        /// <param name="type">The type of the token</param>
        /// <param name="position">The position of the token in the input</param>
        public SplittableToken(ZeroCopyStringView text, TokenType type, int position)
        {
            Text = text;
            Type = type;
            Position = position;
        }
        
        /// <summary>
        /// Split this token into multiple alternatives when ambiguity is detected
        /// </summary>
        /// <param name="alternatives">Array of alternative token text and type pairs</param>
        public void Split(params (ZeroCopyStringView text, TokenType type)[] alternatives)
        {
            Alternatives ??= new List<SplittableToken>();
            
            foreach (var (text, type) in alternatives)
            {
                Alternatives.Add(new SplittableToken(text, type, Position));
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether this token has alternatives
        /// </summary>
        public bool HasAlternatives => Alternatives != null && Alternatives.Count > 0;
    }
    
    /// <summary>
    /// Represents a parsed state from Phase 2 processing
    /// </summary>
    public class ParsedState
    {
        /// <summary>
        /// Gets or sets the token type for this parsed state
        /// </summary>
        public TokenType TokenType { get; init; }
        
        /// <summary>
        /// Gets or sets the text content of the parsed state
        /// </summary>
        public string Text { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the position of the parsed state in the input
        /// </summary>
        public int Position { get; init; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this parsed state is ambiguous
        /// </summary>
        public bool IsAmbiguous { get; init; }
    }
    
    /// <summary>
    /// Enumeration of token types used in both regex pattern parsing and source code tokenization
    /// </summary>
    public enum TokenType
    {
        // Regex pattern tokens
        /// <summary>
        /// Literal character token in regex patterns
        /// </summary>
        Literal,
        
        /// <summary>
        /// Escape sequence token (e.g., \n, \t, \\)
        /// </summary>
        EscapeSequence,
        
        /// <summary>
        /// Character class token (e.g., [a-z], [^0-9])
        /// </summary>
        CharacterClass,
        
        /// <summary>
        /// Group start token (opening parenthesis)
        /// </summary>
        GroupStart,
        
        /// <summary>
        /// Group end token (closing parenthesis)
        /// </summary>
        GroupEnd,
        
        /// <summary>
        /// Special group token (e.g., (?:), (?=), (?!))
        /// </summary>
        SpecialGroup,
        
        /// <summary>
        /// Quantifier token (e.g., *, +, ?, {n,m})
        /// </summary>
        Quantifier,
        
        /// <summary>
        /// Lazy quantifier token (e.g., *?, +?, ??)
        /// </summary>
        LazyQuantifier,
        
        /// <summary>
        /// Alternation token (pipe symbol |)
        /// </summary>
        Alternation,
        
        /// <summary>
        /// Start anchor token (caret ^)
        /// </summary>
        StartAnchor,
        
        /// <summary>
        /// End anchor token (dollar sign $)
        /// </summary>
        EndAnchor,
        
        /// <summary>
        /// Any character token (dot .)
        /// </summary>
        AnyChar,
        
        /// <summary>
        /// Hexadecimal escape token (e.g., \x41)
        /// </summary>
        HexEscape,
        
        /// <summary>
        /// Unicode escape token (e.g., \u0041, \U00000041)
        /// </summary>
        UnicodeEscape,
        
        /// <summary>
        /// Unicode property token (e.g., \p{L}, \P{N})
        /// </summary>
        UnicodeProperty,
        
        // Source code tokens
        /// <summary>
        /// Identifier token in source code
        /// </summary>
        Identifier,
        
        /// <summary>
        /// Numeric literal token
        /// </summary>
        Number,
        
        /// <summary>
        /// String literal token
        /// </summary>
        String,
        
        /// <summary>
        /// Keyword token (language-specific reserved words)
        /// </summary>
        Keyword,
        
        /// <summary>
        /// Operator token (arithmetic, logical, assignment operators)
        /// </summary>
        Operator,
        
        /// <summary>
        /// Whitespace token (spaces, tabs, line breaks)
        /// </summary>
        Whitespace,
        
        /// <summary>
        /// Comment token (single-line and multi-line comments)
        /// </summary>
        Comment,
        
        /// <summary>
        /// Punctuation token (semicolons, commas, brackets)
        /// </summary>
        Punctuation
    }

    /// <summary>
    /// Token produced by the step lexer with position information
    /// </summary>
    public class StepToken
    {
        /// <summary>
        /// Gets or sets the type of the token
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the value/content of the token
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the location of the token in the source code
        /// </summary>
        public ICodeLocation Location { get; set; } = new CodeLocation();
        
        /// <summary>
        /// Gets or sets the context in which the token was found
        /// </summary>
        public string Context { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether this token can be split into alternatives
        /// </summary>
        public bool IsSplittable { get; set; }
        
        /// <summary>
        /// Gets or sets the list of split tokens when ambiguity is resolved
        /// </summary>
        public List<StepToken>? SplitTokens { get; set; }

        /// <summary>
        /// Initializes a new instance of the StepToken class
        /// </summary>
        public StepToken() { }

        /// <summary>
        /// Initializes a new instance of the StepToken class with specified parameters
        /// </summary>
        /// <param name="type">The type of the token</param>
        /// <param name="value">The value/content of the token</param>
        /// <param name="location">The location of the token in the source code</param>
        /// <param name="context">The context in which the token was found</param>
        public StepToken(string type, string value, ICodeLocation location, string context = "")
        {
            Type = type;
            Value = value;
            Location = location;
            Context = context;
        }

        /// <summary>
        /// Returns a string representation of the token
        /// </summary>
        /// <returns>A string in the format "Type:'Value' @Location"</returns>
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
        /// <summary>
        /// Gets or sets the unique identifier for this path
        /// </summary>
        public int PathId { get; set; }
        
        /// <summary>
        /// Gets or sets the current position in the input stream
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// Gets or sets the list of tokens found on this path
        /// </summary>
        public List<StepToken> Tokens { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the current parsing context
        /// </summary>
        public string CurrentContext { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether this path is still valid
        /// </summary>
        public bool IsValid { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the state dictionary for context-specific data
        /// </summary>
        public Dictionary<string, object> State { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the LexerPath class
        /// </summary>
        /// <param name="pathId">The unique identifier for this path</param>
        /// <param name="position">The starting position in the input stream</param>
        public LexerPath(int pathId, int position = 0)
        {
            PathId = pathId;
            Position = position;
        }

        /// <summary>
        /// Creates a clone of this path with a new path ID
        /// </summary>
        /// <param name="newPathId">The path ID for the cloned path</param>
        /// <returns>A new LexerPath instance that is a copy of this path</returns>
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
        /// <summary>
        /// Gets or sets the name of the token rule
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the pattern to match for this rule
        /// </summary>
        public string Pattern { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the context in which this rule applies
        /// </summary>
        public string Context { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the priority of this rule (higher values have higher priority)
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets a value indicating whether this rule is a fragment (used by other rules)
        /// </summary>
        public bool IsFragment { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether tokens matched by this rule should be skipped
        /// </summary>
        public bool IsSkippable { get; set; }
        
        /// <summary>
        /// Gets or sets the action to execute when this rule matches
        /// </summary>
        public Action<StepToken>? Action { get; set; }

        /// <summary>
        /// Initializes a new instance of the TokenRule class
        /// </summary>
        /// <param name="name">The name of the token rule</param>
        /// <param name="pattern">The pattern to match for this rule</param>
        /// <param name="context">The context in which this rule applies</param>
        /// <param name="priority">The priority of this rule</param>
        public TokenRule(string name, string pattern, string context = "", int priority = 0)
        {
            Name = name;
            Pattern = pattern;
            Context = context;
            Priority = priority;
        }
    }

    /// <summary>
    /// Integrated step lexer with two-phase regex pattern parsing and multi-path source code tokenization
    /// Combines regex pattern compilation capabilities with character-by-character tokenization
    /// Implements zero-copy UTF-8 processing and single-pass ambiguity resolution
    /// </summary>
    public class StepLexer
    {
        private readonly List<TokenRule> _rules = new();
        private readonly List<LexerPath> _activePaths = new();
        private readonly IContextStack _contextStack = new ContextStack();
        private ReadOnlyMemory<byte> _input;
        private string _fileName = string.Empty;
        private int _nextPathId = 0;
        
        // Two-phase regex pattern parsing components
        private readonly List<SplittableToken> _phase1Tokens = new();
        private readonly List<ParsedState> _phase2States = new();

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
            
            if (pattern.StartsWith("/") && pattern.EndsWith("/") && pattern.Length > 1)
            {
                // Regex pattern - simplified implementation
                var regex = pattern.Length > 2 ? pattern[1..^1] : "";
                return TryMatchRegex(regex, input);
            }
            else if (pattern.StartsWith("\"") && pattern.EndsWith("\"") && pattern.Length > 1)
            {
                // Literal string match
                var literal = pattern.Length > 2 ? pattern[1..^1] : "";
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
                        var closingBraceIndex = part.IndexOf("}");
                        if (closingBraceIndex > 0) // Make sure it's found and not at position 0
                        {
                            var hex = part.Substring(0, closingBraceIndex);
                            splits.Add(new StepToken("HEX_ESCAPE", $"\\x{{{hex}}}", location, originalToken.Context));
                        }
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
        
        // ===============================================================================================
        // TWO-PHASE REGEX PATTERN PARSING METHODS
        // Integrated from TwoPhaseParser for regex pattern compilation capabilities
        // ===============================================================================================
        
        /// <summary>
        /// Phase 1: Fast lexical scanning with ambiguity detection for regex patterns
        /// </summary>
        public bool Phase1_LexicalScan(ZeroCopyStringView input)
        {
            _phase1Tokens.Clear();
            
            int position = 0;
            while (position < input.Length)
            {
                var result = ScanNextToken(input, position);
                if (result == null)
                    return false;
                    
                _phase1Tokens.Add(result.Value.token);
                position = result.Value.nextPosition;
            }
            
            return true;
        }
        
        /// <summary>
        /// Phase 2: Disambiguation and ENFA state machine construction for regex patterns
        /// </summary>
        public bool Phase2_Disambiguation()
        {
            _phase2States.Clear();
            
            foreach (var token in _phase1Tokens)
            {
                if (token.HasAlternatives)
                {
                    // Handle ambiguous tokens by choosing best alternative
                    var bestAlternative = SelectBestAlternative(token);
                    if (!ProcessRegexToken(bestAlternative))
                        return false;
                }
                else
                {
                    if (!ProcessRegexToken(token))
                        return false;
                }
            }
            
            return true;
        }
        
        private (SplittableToken token, int nextPosition)? ScanNextToken(ZeroCopyStringView input, int position)
        {
            if (position >= input.Length)
                return null;
                
            byte currentByte = input[position];
            
            // Fast pattern recognition for common cases
            switch (currentByte)
            {
                case (byte)'\\':
                    return ScanEscapeSequence(input, position);
                case (byte)'[':
                    return ScanCharacterClass(input, position);
                case (byte)'(':
                    return ScanGroup(input, position);
                case (byte)')':
                    return (new SplittableToken(input.Slice(position, 1), TokenType.GroupEnd, position), position + 1);
                case (byte)'*':
                case (byte)'+':
                case (byte)'?':
                    return ScanQuantifier(input, position);
                case (byte)'|':
                    return (new SplittableToken(input.Slice(position, 1), TokenType.Alternation, position), position + 1);
                case (byte)'^':
                    return (new SplittableToken(input.Slice(position, 1), TokenType.StartAnchor, position), position + 1);
                case (byte)'$':
                    return (new SplittableToken(input.Slice(position, 1), TokenType.EndAnchor, position), position + 1);
                case (byte)'.':
                    return (new SplittableToken(input.Slice(position, 1), TokenType.AnyChar, position), position + 1);
                default:
                    return ScanLiteral(input, position);
            }
        }
        
        private (SplittableToken token, int nextPosition)? ScanEscapeSequence(ZeroCopyStringView input, int position)
        {
            if (position + 1 >= input.Length)
                return null;
                
            byte nextByte = input[position + 1];
            var token = new SplittableToken(input.Slice(position, 2), TokenType.EscapeSequence, position);
            
            // Check for potential ambiguity in escape sequences
            switch (nextByte)
            {
                case (byte)'x':
                    // Could be \xFF or \x{FFFF} - ambiguous!
                    // Only create alternatives if we have enough input
                    if (position + 4 <= input.Length)
                    {
                        token.Split(
                            (input.Slice(position, 4), TokenType.HexEscape),  // \xFF
                            (position + 6 <= input.Length ? input.Slice(position, 6) : input.Slice(position, Math.Min(4, input.Length - position)), TokenType.UnicodeEscape) // \x{FF}
                        );
                    }
                    return (token, position + 2);
                    
                case (byte)'p':
                case (byte)'P':
                    // Unicode property - scan full property name
                    return ScanUnicodeProperty(input, position);
                    
                default:
                    return (token, position + 2);
            }
        }
        
        private (SplittableToken token, int nextPosition)? ScanCharacterClass(ZeroCopyStringView input, int position)
        {
            int end = position + 1;
            bool escaped = false;
            
            while (end < input.Length)
            {
                byte b = input[end];
                if (!escaped && b == (byte)']')
                    break;
                escaped = !escaped && b == (byte)'\\';
                end++;
            }
            
            if (end >= input.Length)
                return null; // Unclosed character class
                
            var token = new SplittableToken(input.Slice(position, end - position + 1), TokenType.CharacterClass, position);
            return (token, end + 1);
        }
        
        private (SplittableToken token, int nextPosition)? ScanGroup(ZeroCopyStringView input, int position)
        {
            // Check for special group types like (?:...) or (?=...)
            if (position + 1 < input.Length && input[position + 1] == (byte)'?')
            {
                var token = new SplittableToken(input.Slice(position, 2), TokenType.SpecialGroup, position);
                return (token, position + 2);
            }
            
            var groupToken = new SplittableToken(input.Slice(position, 1), TokenType.GroupStart, position);
            return (groupToken, position + 1);
        }
        
        private (SplittableToken token, int nextPosition)? ScanQuantifier(ZeroCopyStringView input, int position)
        {
            var token = new SplittableToken(input.Slice(position, 1), TokenType.Quantifier, position);
            
            // Check for lazy quantifiers (+?, *?, ??)
            if (position + 1 < input.Length && input[position + 1] == (byte)'?')
            {
                token = new SplittableToken(input.Slice(position, 2), TokenType.LazyQuantifier, position);
                return (token, position + 2);
            }
            
            return (token, position + 1);
        }
        
        private (SplittableToken token, int nextPosition)? ScanLiteral(ZeroCopyStringView input, int position)
        {
            var token = new SplittableToken(input.Slice(position, 1), TokenType.Literal, position);
            return (token, position + 1);
        }
        
        private (SplittableToken token, int nextPosition)? ScanUnicodeProperty(ZeroCopyStringView input, int position)
        {
            // Scan for \p{PropertyName} or \P{PropertyName}
            int end = position + 2; // Skip \p or \P
            
            if (end < input.Length && input[end] == (byte)'{')
            {
                end++; // Skip {
                while (end < input.Length && input[end] != (byte)'}')
                    end++;
                    
                if (end < input.Length)
                    end++; // Include }
            }
            
            var token = new SplittableToken(input.Slice(position, end - position), TokenType.UnicodeProperty, position);
            return (token, end);
        }
        
        private SplittableToken SelectBestAlternative(SplittableToken ambiguousToken)
        {
            // Simple heuristic: prefer longer matches
            if (ambiguousToken.Alternatives == null)
                return ambiguousToken;
                
            var best = ambiguousToken;
            foreach (var alternative in ambiguousToken.Alternatives)
            {
                if (alternative.Text.Length > best.Text.Length)
                    best = alternative;
            }
            
            return best;
        }
        
        private bool ProcessRegexToken(SplittableToken token)
        {
            // Convert processed token to parsed state for regex patterns
            var state = new ParsedState
            {
                TokenType = token.Type,
                Text = token.Text.ToString(),
                Position = token.Position,
                IsAmbiguous = token.HasAlternatives
            };
            
            _phase2States.Add(state);
            return true;
        }
        
        /// <summary>
        /// Access to Phase 1 regex parsing results
        /// </summary>
        public IReadOnlyList<SplittableToken> Phase1Results => _phase1Tokens;
        
        /// <summary>
        /// Access to Phase 2 regex parsing results
        /// </summary>
        public IReadOnlyList<ParsedState> Phase2Results => _phase2States;
    }

    /// <summary>
    /// Result of a single lexer step
    /// </summary>
    public class LexerStepResult
    {
        /// <summary>
        /// Gets or sets the list of new tokens created during this step
        /// </summary>
        public List<StepToken> NewTokens { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of context changes that occurred during this step
        /// </summary>
        public List<string> ContextChanges { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the number of active parsing paths
        /// </summary>
        public int ActivePathCount { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether parsing is complete
        /// </summary>
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Result of processing a single path
    /// </summary>
    public class PathStepResult
    {
        /// <summary>
        /// Gets or sets the list of tokens found on this path
        /// </summary>
        public List<StepToken> Tokens { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of paths resulting from this processing step
        /// </summary>
        public List<LexerPath> Paths { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of context changes that occurred on this path
        /// </summary>
        public List<string> ContextChanges { get; set; } = new();
    }
}