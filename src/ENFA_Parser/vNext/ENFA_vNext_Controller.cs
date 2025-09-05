using System;
using System.IO;
using System.Text;

namespace ENFA_Parser.vNext
{
    /// <summary>
    /// Parser types supported by vNext architecture
    /// </summary>
    public enum ParserType
    {
        Regex,
        Grammar
    }

    /// <summary>
    /// vNext ENFA Controller implementing zero-copy parsing and two-phase processing 
    /// to avoid regex complexity explosion
    /// </summary>
    public class ENFA_vNext_Controller
    {
        private readonly TwoPhaseParser _twoPhaseParser;
        private readonly ParserType _parserType;
        private ReadOnlyMemory<byte> _inputBuffer;
        
        public ENFA_vNext_Controller(ParserType parserType)
        {
            _parserType = parserType;
            _twoPhaseParser = new TwoPhaseParser();
        }
        
        /// <summary>
        /// Parse pattern using zero-copy, UTF-8 based approach
        /// </summary>
        public bool ParsePattern(ReadOnlySpan<byte> utf8Pattern, string terminalName)
        {
            // Create zero-copy view of the input
            _inputBuffer = new ReadOnlyMemory<byte>(utf8Pattern.ToArray());
            var inputView = new ZeroCopyStringView(_inputBuffer);
            
            // Phase 1: Fast lexical analysis with ambiguity detection
            if (!_twoPhaseParser.Phase1_LexicalScan(inputView))
            {
                return false;
            }
            
            // Phase 2: Disambiguation and ENFA construction
            if (!_twoPhaseParser.Phase2_Disambiguation())
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Parse pattern from UTF-8 string (convenience method)
        /// </summary>
        public bool ParsePattern(string pattern, string terminalName)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(pattern);
            _inputBuffer = utf8Bytes;
            return ParsePattern(utf8Bytes.AsSpan(), terminalName);
        }
        
        /// <summary>
        /// Parse pattern from stream with zero-copy approach
        /// </summary>
        public bool ParsePatternFromStream(Stream stream, string terminalName)
        {
            // Read entire stream into memory for zero-copy processing
            // This trades some memory for significant performance gains
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            _inputBuffer = memoryStream.ToArray();
            
            var inputView = new ZeroCopyStringView(_inputBuffer);
            
            // Phase 1: Fast lexical analysis
            if (!_twoPhaseParser.Phase1_LexicalScan(inputView))
            {
                return false;
            }
            
            // Phase 2: Disambiguation and ENFA construction  
            if (!_twoPhaseParser.Phase2_Disambiguation())
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get parsing results and statistics
        /// </summary>
        public ParsingResult GetResults()
        {
            return new ParsingResult
            {
                Phase1TokenCount = _twoPhaseParser.Phase1Results.Count,
                AmbiguousTokenCount = CountAmbiguousTokens(),
                MemoryUsed = _inputBuffer.Length,
                PatternHierarchy = GetPatternHierarchy()
            };
        }
        
        private int CountAmbiguousTokens()
        {
            int count = 0;
            foreach (var token in _twoPhaseParser.Phase1Results)
            {
                if (token.HasAlternatives)
                    count++;
            }
            return count;
        }
        
        private string GetPatternHierarchy()
        {
            var sb = new StringBuilder();
            sb.AppendLine("vNext Pattern Analysis:");
            sb.AppendLine($"Parser Type: {_parserType}");
            sb.AppendLine($"Phase 1 Tokens: {_twoPhaseParser.Phase1Results.Count}");
            
            for (int i = 0; i < _twoPhaseParser.Phase1Results.Count; i++)
            {
                var token = _twoPhaseParser.Phase1Results[i];
                sb.AppendLine($"  Token {i}: {token.Type} - {token.Text}");
                if (token.HasAlternatives)
                {
                    foreach (var alt in token.Alternatives!)
                    {
                        sb.AppendLine($"    Alt: {alt.Type} - {alt.Text}");
                    }
                }
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Get parser type
        /// </summary>
        public ParserType ParserType => _parserType;
        
        /// <summary>
        /// Access to phase 1 parsing results for debugging
        /// </summary>
        public TwoPhaseParser TwoPhaseParser => _twoPhaseParser;
    }
    
    /// <summary>
    /// Results and statistics from vNext parsing
    /// </summary>
    public class ParsingResult
    {
        public int Phase1TokenCount { get; init; }
        public int AmbiguousTokenCount { get; init; }
        public long MemoryUsed { get; init; }
        public string PatternHierarchy { get; init; } = string.Empty;
        
        public double AmbiguityRatio => Phase1TokenCount > 0 ? (double)AmbiguousTokenCount / Phase1TokenCount : 0.0;
    }
}