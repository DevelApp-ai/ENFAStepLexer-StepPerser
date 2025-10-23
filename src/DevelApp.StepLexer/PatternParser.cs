using System;
using System.IO;
using System.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Parser types supported by vNext architecture
    /// </summary>
    public enum ParserType
    {
        /// <summary>
        /// Regular expression pattern parsing
        /// </summary>
        Regex,
        
        /// <summary>
        /// Grammar-based pattern parsing
        /// </summary>
        Grammar
    }

    /// <summary>
    /// Parser controller implementing zero-copy parsing and two-phase processing 
    /// to avoid regex complexity explosion
    /// </summary>
    public class PatternParser
    {
        private readonly StepLexer _stepLexer;
        private readonly ParserType _parserType;
        private ReadOnlyMemory<byte> _inputBuffer;
        
        /// <summary>
        /// Initializes a new instance of the PatternParser class with the specified parser type
        /// </summary>
        /// <param name="parserType">The type of parser to use (Regex or Grammar)</param>
        public PatternParser(ParserType parserType)
        {
            _parserType = parserType;
            _stepLexer = new StepLexer();
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
            if (!_stepLexer.Phase1_LexicalScan(inputView))
            {
                return false;
            }
            
            // Phase 2: Disambiguation and ENFA construction
            if (!_stepLexer.Phase2_Disambiguation())
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
        /// Parse pattern from bytes with specified source encoding, converting to UTF-8
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="sourceEncoding">Source encoding object (e.g., Encoding.UTF8, Encoding.GetEncoding("shift_jis"))</param>
        /// <param name="terminalName">Terminal name for the pattern</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public bool ParsePattern(ReadOnlySpan<byte> sourceBytes, Encoding sourceEncoding, string terminalName)
        {
            // Convert to UTF-8 using the library
            byte[] utf8Bytes = EncodingConverter.ConvertToUTF8(sourceBytes, sourceEncoding);
            
            _inputBuffer = utf8Bytes;
            return ParsePattern(utf8Bytes.AsSpan(), terminalName);
        }
        
        /// <summary>
        /// Parse pattern from bytes with encoding specified by name
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="encodingName">Encoding name (e.g., "UTF-16", "ISO-8859-1", "shift_jis", "GB2312")</param>
        /// <param name="terminalName">Terminal name for the pattern</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public bool ParsePattern(ReadOnlySpan<byte> sourceBytes, string encodingName, string terminalName)
        {
            // Convert to UTF-8 using the library
            byte[] utf8Bytes = EncodingConverter.ConvertToUTF8(sourceBytes, encodingName);
            
            _inputBuffer = utf8Bytes;
            return ParsePattern(utf8Bytes.AsSpan(), terminalName);
        }
        
        /// <summary>
        /// Parse pattern from bytes with encoding specified by code page
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="codePage">Code page number (e.g., 1252 for Windows-1252, 28591 for ISO-8859-1)</param>
        /// <param name="terminalName">Terminal name for the pattern</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public bool ParsePattern(ReadOnlySpan<byte> sourceBytes, int codePage, string terminalName)
        {
            // Convert to UTF-8 using the library
            byte[] utf8Bytes = EncodingConverter.ConvertToUTF8(sourceBytes, codePage);
            
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
            if (!_stepLexer.Phase1_LexicalScan(inputView))
            {
                return false;
            }
            
            // Phase 2: Disambiguation and ENFA construction  
            if (!_stepLexer.Phase2_Disambiguation())
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Parse pattern from stream with specified encoding
        /// </summary>
        /// <param name="stream">Input stream containing the pattern</param>
        /// <param name="sourceEncoding">Source encoding object</param>
        /// <param name="terminalName">Terminal name for the pattern</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public bool ParsePatternFromStream(Stream stream, Encoding sourceEncoding, string terminalName)
        {
            // Read entire stream into memory
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var sourceBytes = memoryStream.ToArray();
            
            // Convert to UTF-8 using the library
            byte[] utf8Bytes = EncodingConverter.ConvertToUTF8(sourceBytes, sourceEncoding);
            
            _inputBuffer = utf8Bytes;
            var inputView = new ZeroCopyStringView(_inputBuffer);
            
            // Phase 1: Fast lexical analysis
            if (!_stepLexer.Phase1_LexicalScan(inputView))
            {
                return false;
            }
            
            // Phase 2: Disambiguation and ENFA construction
            if (!_stepLexer.Phase2_Disambiguation())
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Parse pattern from stream with automatic encoding detection from BOM
        /// </summary>
        /// <param name="stream">Input stream containing the pattern</param>
        /// <param name="terminalName">Terminal name for the pattern</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public bool ParsePatternFromStreamWithAutoDetect(Stream stream, string terminalName)
        {
            // Read entire stream into memory
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var sourceBytes = memoryStream.ToArray();
            
            // Auto-detect encoding and convert to UTF-8
            byte[] utf8Bytes = EncodingConverter.ConvertToUTF8WithAutoDetect(sourceBytes);
            
            _inputBuffer = utf8Bytes;
            var inputView = new ZeroCopyStringView(_inputBuffer);
            
            // Phase 1: Fast lexical analysis
            if (!_stepLexer.Phase1_LexicalScan(inputView))
            {
                return false;
            }
            
            // Phase 2: Disambiguation and ENFA construction
            if (!_stepLexer.Phase2_Disambiguation())
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
                Phase1TokenCount = _stepLexer.Phase1Results.Count,
                AmbiguousTokenCount = CountAmbiguousTokens(),
                MemoryUsed = _inputBuffer.Length,
                PatternHierarchy = GetPatternHierarchy()
            };
        }
        
        private int CountAmbiguousTokens()
        {
            int count = 0;
            foreach (var token in _stepLexer.Phase1Results)
            {
                if (token.HasAlternatives)
                    count++;
            }
            return count;
        }
        
        private string GetPatternHierarchy()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Pattern Analysis:");
            sb.AppendLine($"Parser Type: {_parserType}");
            sb.AppendLine($"Phase 1 Tokens: {_stepLexer.Phase1Results.Count}");
            
            for (int i = 0; i < _stepLexer.Phase1Results.Count; i++)
            {
                var token = _stepLexer.Phase1Results[i];
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
        public StepLexer StepLexer => _stepLexer;
    }
    
    /// <summary>
    /// Results and statistics from parsing
    /// </summary>
    public class ParsingResult
    {
        /// <summary>
        /// Gets the number of tokens generated in phase 1 parsing
        /// </summary>
        public int Phase1TokenCount { get; init; }
        
        /// <summary>
        /// Gets the number of ambiguous tokens that required splitting
        /// </summary>
        public int AmbiguousTokenCount { get; init; }
        
        /// <summary>
        /// Gets the amount of memory used during parsing in bytes
        /// </summary>
        public long MemoryUsed { get; init; }
        
        /// <summary>
        /// Gets the pattern hierarchy as a string representation
        /// </summary>
        public string PatternHierarchy { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets the ratio of ambiguous tokens to total tokens (0.0 to 1.0)
        /// </summary>
        public double AmbiguityRatio => Phase1TokenCount > 0 ? (double)AmbiguousTokenCount / Phase1TokenCount : 0.0;
    }
}