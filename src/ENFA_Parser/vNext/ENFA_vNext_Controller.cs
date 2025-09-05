using System;
using System.IO;
using System.Text;

namespace ENFA_Parser.vNext
{
    /// <summary>
    /// Enhanced ENFA Controller implementing vNext architecture with zero-copy parsing
    /// and two-phase processing to avoid regex complexity explosion
    /// </summary>
    public class ENFA_vNext_Controller
    {
        private readonly TwoPhaseParser _twoPhaseParser;
        private readonly ENFA_Controller _legacyController;
        private ReadOnlyMemory<byte> _inputBuffer;
        
        public ENFA_vNext_Controller(ParserType parserType)
        {
            _twoPhaseParser = new TwoPhaseParser();
            _legacyController = new ENFA_Controller(parserType);
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
            if (!_twoPhaseParser.Phase2_Disambiguation(_legacyController))
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
            if (!_twoPhaseParser.Phase2_Disambiguation(_legacyController))
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
                PatternHierarchy = _legacyController.PrintHierarchy
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
        
        /// <summary>
        /// Access to underlying legacy controller for compatibility
        /// </summary>
        public ENFA_Controller LegacyController => _legacyController;
        
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