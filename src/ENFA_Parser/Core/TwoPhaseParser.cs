using System;
using System.Collections.Generic;

namespace ENFA_Parser.Core
{
    /// <summary>
    /// Represents a token that can be split into multiple alternatives during ambiguity resolution
    /// </summary>
    public class SplittableToken
    {
        public ZeroCopyStringView Text { get; }
        public TokenType Type { get; set; }
        public int Position { get; }
        public List<SplittableToken>? Alternatives { get; set; }
        
        public SplittableToken(ZeroCopyStringView text, TokenType type, int position)
        {
            Text = text;
            Type = type;
            Position = position;
        }
        
        /// <summary>
        /// Split this token into multiple alternatives when ambiguity is detected
        /// </summary>
        public void Split(params (ZeroCopyStringView text, TokenType type)[] alternatives)
        {
            Alternatives ??= new List<SplittableToken>();
            
            foreach (var (text, type) in alternatives)
            {
                Alternatives.Add(new SplittableToken(text, type, Position));
            }
        }
        
        public bool HasAlternatives => Alternatives != null && Alternatives.Count > 0;
    }
    
    /// <summary>
    /// Two-phase parsing approach to avoid regex complexity explosion
    /// Phase 1: Rapid lexical analysis with ambiguity detection
    /// Phase 2: Disambiguation and pattern compilation
    /// </summary>
    public class TwoPhaseParser
    {
        private readonly List<SplittableToken> _phase1Tokens = new();
        private readonly List<ParsedState> _phase2States = new();
        
        /// <summary>
        /// Phase 1: Fast lexical scanning with ambiguity detection
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
        /// Phase 2: Disambiguation and ENFA state machine construction
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
                    if (!ProcessToken(bestAlternative))
                        return false;
                }
                else
                {
                    if (!ProcessToken(token))
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
        
        private bool ProcessToken(SplittableToken token)
        {
            // Convert processed token to vNext parsed state
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
        
        public IReadOnlyList<SplittableToken> Phase1Results => _phase1Tokens;
        public IReadOnlyList<ParsedState> Phase2Results => _phase2States;
    }
    
    /// <summary>
    /// Represents a parsed state from Phase 2 processing
    /// </summary>
    public class ParsedState
    {
        public TokenType TokenType { get; init; }
        public string Text { get; init; } = string.Empty;
        public int Position { get; init; }
        public bool IsAmbiguous { get; init; }
    }
    
    public enum TokenType
    {
        Literal,
        EscapeSequence,
        CharacterClass,
        GroupStart,
        GroupEnd,
        SpecialGroup,
        Quantifier,
        LazyQuantifier,
        Alternation,
        StartAnchor,
        EndAnchor,
        AnyChar,
        HexEscape,
        UnicodeEscape,
        UnicodeProperty
    }
}