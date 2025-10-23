using System;
using System.Buffers;
using System.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Supported source encodings for efficient conversion to UTF-8
    /// </summary>
    public enum SourceEncoding
    {
        /// <summary>UTF-8 (no conversion needed)</summary>
        UTF8,
        /// <summary>ASCII (7-bit)</summary>
        ASCII,
        /// <summary>UTF-16 Little Endian</summary>
        UTF16LE,
        /// <summary>UTF-16 Big Endian</summary>
        UTF16BE,
        /// <summary>UTF-32 Little Endian</summary>
        UTF32LE,
        /// <summary>UTF-32 Big Endian</summary>
        UTF32BE,
        /// <summary>ISO-8859-1 (Latin-1)</summary>
        Latin1,
        /// <summary>Windows-1252 (Western European)</summary>
        Windows1252,
        /// <summary>Auto-detect from BOM or content</summary>
        AutoDetect
    }

    /// <summary>
    /// Efficient encoding converter for converting various formats to UTF-8
    /// </summary>
    public static class EncodingConverter
    {
        /// <summary>
        /// Convert from various encodings to UTF-8 efficiently
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="sourceEncoding">Source encoding type</param>
        /// <returns>UTF-8 encoded bytes</returns>
        public static byte[] ConvertToUTF8(ReadOnlySpan<byte> sourceBytes, SourceEncoding sourceEncoding)
        {
            if (sourceBytes.IsEmpty)
                return Array.Empty<byte>();

            return sourceEncoding switch
            {
                SourceEncoding.UTF8 => sourceBytes.ToArray(),
                SourceEncoding.ASCII => ConvertAsciiToUTF8(sourceBytes),
                SourceEncoding.UTF16LE => ConvertUTF16LEToUTF8(sourceBytes),
                SourceEncoding.UTF16BE => ConvertUTF16BEToUTF8(sourceBytes),
                SourceEncoding.UTF32LE => ConvertUTF32LEToUTF8(sourceBytes),
                SourceEncoding.UTF32BE => ConvertUTF32BEToUTF8(sourceBytes),
                SourceEncoding.Latin1 => ConvertLatin1ToUTF8(sourceBytes),
                SourceEncoding.Windows1252 => ConvertWindows1252ToUTF8(sourceBytes),
                SourceEncoding.AutoDetect => ConvertWithAutoDetect(sourceBytes),
                _ => throw new NotSupportedException($"Encoding {sourceEncoding} is not supported")
            };
        }

        /// <summary>
        /// Detect encoding from BOM (Byte Order Mark) or content analysis
        /// </summary>
        public static SourceEncoding DetectEncoding(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 0)
                return SourceEncoding.UTF8;

            // Check for BOM
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return SourceEncoding.UTF8;
            
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                if (bytes.Length >= 4 && bytes[2] == 0x00 && bytes[3] == 0x00)
                    return SourceEncoding.UTF32LE;
                return SourceEncoding.UTF16LE;
            }
            
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return SourceEncoding.UTF16BE;
            
            if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
                return SourceEncoding.UTF32BE;

            // Heuristic detection (simplified)
            bool hasNulls = false;
            bool allAscii = true;
            
            for (int i = 0; i < Math.Min(bytes.Length, 1024); i++)
            {
                if (bytes[i] == 0)
                    hasNulls = true;
                if (bytes[i] > 127)
                    allAscii = false;
            }

            if (allAscii)
                return SourceEncoding.ASCII;
            if (hasNulls)
                return SourceEncoding.UTF16LE; // Likely UTF-16 or UTF-32
            
            // Default to UTF-8
            return SourceEncoding.UTF8;
        }

        private static byte[] ConvertWithAutoDetect(ReadOnlySpan<byte> sourceBytes)
        {
            var detected = DetectEncoding(sourceBytes);
            return ConvertToUTF8(sourceBytes, detected);
        }

        private static byte[] ConvertAsciiToUTF8(ReadOnlySpan<byte> asciiBytes)
        {
            // ASCII is a subset of UTF-8, direct copy
            return asciiBytes.ToArray();
        }

        private static byte[] ConvertLatin1ToUTF8(ReadOnlySpan<byte> latin1Bytes)
        {
            // Estimate size: Latin-1 chars 0x80-0xFF need 2 bytes in UTF-8
            int maxSize = latin1Bytes.Length * 2;
            byte[] result = new byte[maxSize];
            int writePos = 0;

            for (int i = 0; i < latin1Bytes.Length; i++)
            {
                byte b = latin1Bytes[i];
                
                if (b < 0x80)
                {
                    // ASCII range: direct copy
                    result[writePos++] = b;
                }
                else
                {
                    // 0x80-0xFF: encode as 2-byte UTF-8
                    result[writePos++] = (byte)(0xC0 | (b >> 6));
                    result[writePos++] = (byte)(0x80 | (b & 0x3F));
                }
            }

            Array.Resize(ref result, writePos);
            return result;
        }

        private static byte[] ConvertWindows1252ToUTF8(ReadOnlySpan<byte> windows1252Bytes)
        {
            // Windows-1252 is similar to Latin-1 but with special mappings in 0x80-0x9F
            // Try to get the encoding, fall back to Latin-1 if not available
            try
            {
                var encoding = Encoding.GetEncoding(1252);
                var chars = encoding.GetString(windows1252Bytes.ToArray());
                return Encoding.UTF8.GetBytes(chars);
            }
            catch (NotSupportedException)
            {
                // Fall back to Latin-1 if Windows-1252 is not available
                return ConvertLatin1ToUTF8(windows1252Bytes);
            }
        }

        private static byte[] ConvertUTF16LEToUTF8(ReadOnlySpan<byte> utf16Bytes)
        {
            if (utf16Bytes.Length % 2 != 0)
                throw new ArgumentException("UTF-16 input must have even number of bytes");

            // Skip BOM if present
            int startIndex = 0;
            if (utf16Bytes.Length >= 2 && utf16Bytes[0] == 0xFF && utf16Bytes[1] == 0xFE)
                startIndex = 2;

            var chars = System.Text.Encoding.Unicode.GetString(utf16Bytes.Slice(startIndex).ToArray());
            return Encoding.UTF8.GetBytes(chars);
        }

        private static byte[] ConvertUTF16BEToUTF8(ReadOnlySpan<byte> utf16Bytes)
        {
            if (utf16Bytes.Length % 2 != 0)
                throw new ArgumentException("UTF-16 input must have even number of bytes");

            // Skip BOM if present
            int startIndex = 0;
            if (utf16Bytes.Length >= 2 && utf16Bytes[0] == 0xFE && utf16Bytes[1] == 0xFF)
                startIndex = 2;

            var chars = System.Text.Encoding.BigEndianUnicode.GetString(utf16Bytes.Slice(startIndex).ToArray());
            return Encoding.UTF8.GetBytes(chars);
        }

        private static byte[] ConvertUTF32LEToUTF8(ReadOnlySpan<byte> utf32Bytes)
        {
            if (utf32Bytes.Length % 4 != 0)
                throw new ArgumentException("UTF-32 input must have length divisible by 4");

            // Skip BOM if present
            int startIndex = 0;
            if (utf32Bytes.Length >= 4 && utf32Bytes[0] == 0xFF && utf32Bytes[1] == 0xFE && 
                utf32Bytes[2] == 0x00 && utf32Bytes[3] == 0x00)
                startIndex = 4;

            var chars = System.Text.Encoding.UTF32.GetString(utf32Bytes.Slice(startIndex).ToArray());
            return Encoding.UTF8.GetBytes(chars);
        }

        private static byte[] ConvertUTF32BEToUTF8(ReadOnlySpan<byte> utf32Bytes)
        {
            if (utf32Bytes.Length % 4 != 0)
                throw new ArgumentException("UTF-32 input must have length divisible by 4");

            // Skip BOM if present
            int startIndex = 0;
            if (utf32Bytes.Length >= 4 && utf32Bytes[0] == 0x00 && utf32Bytes[1] == 0x00 && 
                utf32Bytes[2] == 0xFE && utf32Bytes[3] == 0xFF)
                startIndex = 4;

            // Convert BE to LE by swapping bytes
            byte[] leBytes = new byte[utf32Bytes.Length - startIndex];
            for (int i = startIndex; i < utf32Bytes.Length; i += 4)
            {
                leBytes[i - startIndex + 3] = utf32Bytes[i];
                leBytes[i - startIndex + 2] = utf32Bytes[i + 1];
                leBytes[i - startIndex + 1] = utf32Bytes[i + 2];
                leBytes[i - startIndex] = utf32Bytes[i + 3];
            }

            var chars = System.Text.Encoding.UTF32.GetString(leBytes);
            return Encoding.UTF8.GetBytes(chars);
        }
    }

    /// <summary>
    /// UTF-8 processing utilities to avoid UTF-16 conversion overhead
    /// </summary>
    public static class UTF8Utils
    {
        /// <summary>
        /// Check if a UTF-8 byte sequence represents a specific ASCII character
        /// </summary>
        public static bool IsAsciiChar(ReadOnlySpan<byte> utf8Bytes, int position, char asciiChar)
        {
            if (position >= utf8Bytes.Length || asciiChar > 127)
                return false;
                
            return utf8Bytes[position] == (byte)asciiChar;
        }
        
        /// <summary>
        /// Get the next UTF-8 codepoint from the byte stream
        /// Returns the codepoint and number of bytes consumed
        /// </summary>
        public static (uint codepoint, int bytesConsumed) GetNextCodepoint(ReadOnlySpan<byte> utf8Bytes, int position)
        {
            if (position >= utf8Bytes.Length)
                return (0, 0);
                
            byte firstByte = utf8Bytes[position];
            
            // ASCII (0xxxxxxx)
            if ((firstByte & 0x80) == 0)
            {
                return (firstByte, 1);
            }
            
            // 2-byte sequence (110xxxxx 10xxxxxx)
            if ((firstByte & 0xE0) == 0xC0)
            {
                if (position + 1 >= utf8Bytes.Length)
                    return (0xFFFD, 1); // Replacement character for invalid UTF-8
                    
                byte secondByte = utf8Bytes[position + 1];
                if ((secondByte & 0xC0) != 0x80)
                    return (0xFFFD, 1);
                    
                uint codepoint = ((uint)(firstByte & 0x1F) << 6) | (uint)(secondByte & 0x3F);
                return (codepoint, 2);
            }
            
            // 3-byte sequence (1110xxxx 10xxxxxx 10xxxxxx)
            if ((firstByte & 0xF0) == 0xE0)
            {
                if (position + 2 >= utf8Bytes.Length)
                    return (0xFFFD, 1);
                    
                byte secondByte = utf8Bytes[position + 1];
                byte thirdByte = utf8Bytes[position + 2];
                
                if ((secondByte & 0xC0) != 0x80 || (thirdByte & 0xC0) != 0x80)
                    return (0xFFFD, 1);
                    
                uint codepoint = ((uint)(firstByte & 0x0F) << 12) | 
                               ((uint)(secondByte & 0x3F) << 6) | 
                               (uint)(thirdByte & 0x3F);
                return (codepoint, 3);
            }
            
            // 4-byte sequence (11110xxx 10xxxxxx 10xxxxxx 10xxxxxx)
            if ((firstByte & 0xF8) == 0xF0)
            {
                if (position + 3 >= utf8Bytes.Length)
                    return (0xFFFD, 1);
                    
                byte secondByte = utf8Bytes[position + 1];
                byte thirdByte = utf8Bytes[position + 2];
                byte fourthByte = utf8Bytes[position + 3];
                
                if ((secondByte & 0xC0) != 0x80 || (thirdByte & 0xC0) != 0x80 || (fourthByte & 0xC0) != 0x80)
                    return (0xFFFD, 1);
                    
                uint codepoint = ((uint)(firstByte & 0x07) << 18) | 
                               ((uint)(secondByte & 0x3F) << 12) | 
                               ((uint)(thirdByte & 0x3F) << 6) | 
                               (uint)(fourthByte & 0x3F);
                return (codepoint, 4);
            }
            
            // Invalid UTF-8
            return (0xFFFD, 1);
        }
        
        /// <summary>
        /// Fast ASCII-only pattern matching for common regex constructs
        /// </summary>
        public static bool MatchesAsciiPattern(ReadOnlySpan<byte> utf8Bytes, int position, ReadOnlySpan<byte> pattern)
        {
            if (position + pattern.Length > utf8Bytes.Length)
                return false;
                
            return utf8Bytes.Slice(position, pattern.Length).SequenceEqual(pattern);
        }
        
        /// <summary>
        /// Check if byte is ASCII whitespace
        /// </summary>
        public static bool IsAsciiWhitespace(byte b)
        {
            return b == 0x20 || // space
                   b == 0x09 || // tab
                   b == 0x0A || // line feed
                   b == 0x0D || // carriage return
                   b == 0x0C || // form feed
                   b == 0x0B;   // vertical tab
        }
        
        /// <summary>
        /// Check if byte is ASCII digit
        /// </summary>
        public static bool IsAsciiDigit(byte b)
        {
            return b >= 0x30 && b <= 0x39; // '0' to '9'
        }
        
        /// <summary>
        /// Check if byte is ASCII letter
        /// </summary>
        public static bool IsAsciiLetter(byte b)
        {
            return (b >= 0x41 && b <= 0x5A) || // 'A' to 'Z'
                   (b >= 0x61 && b <= 0x7A);   // 'a' to 'z'
        }
        
        /// <summary>
        /// Convert hex digit to value (for \xHH patterns)
        /// </summary>
        public static int HexDigitToValue(byte b)
        {
            if (b >= 0x30 && b <= 0x39) // '0' to '9'
                return b - 0x30;
            if (b >= 0x41 && b <= 0x46) // 'A' to 'F'
                return b - 0x41 + 10;
            if (b >= 0x61 && b <= 0x66) // 'a' to 'f'
                return b - 0x61 + 10;
            return -1; // Invalid hex digit
        }
        
        /// <summary>
        /// Parse hex escape sequence (\xHH or \x{HHHH})
        /// </summary>
        public static (uint codepoint, int bytesConsumed) ParseHexEscape(ReadOnlySpan<byte> utf8Bytes, int position)
        {
            if (position + 2 >= utf8Bytes.Length || 
                utf8Bytes[position] != (byte)'\\' || 
                utf8Bytes[position + 1] != (byte)'x')
                return (0, 0);
                
            int pos = position + 2;
            
            // Check for \x{HHHH} format
            if (pos < utf8Bytes.Length && utf8Bytes[pos] == (byte)'{')
            {
                pos++; // Skip '{'
                uint codepoint = 0;
                int hexDigits = 0;
                
                while (pos < utf8Bytes.Length && utf8Bytes[pos] != (byte)'}' && hexDigits < 6)
                {
                    int hexValue = HexDigitToValue(utf8Bytes[pos]);
                    if (hexValue == -1)
                        break;
                        
                    codepoint = (codepoint << 4) | (uint)hexValue;
                    hexDigits++;
                    pos++;
                }
                
                if (pos < utf8Bytes.Length && utf8Bytes[pos] == (byte)'}' && hexDigits > 0)
                {
                    return (codepoint, pos - position + 1);
                }
                
                return (0, 0); // Invalid format
            }
            
            // Standard \xHH format
            if (pos + 1 < utf8Bytes.Length)
            {
                int high = HexDigitToValue(utf8Bytes[pos]);
                int low = HexDigitToValue(utf8Bytes[pos + 1]);
                
                if (high != -1 && low != -1)
                {
                    uint codepoint = (uint)((high << 4) | low);
                    return (codepoint, 4); // \xHH
                }
            }
            
            return (0, 0); // Invalid format
        }
    }
    
    /// <summary>
    /// Memory-efficient string builder for UTF-8 that avoids allocations
    /// </summary>
    public ref struct UTF8StringBuilder
    {
        private Span<byte> _buffer;
        private int _position;
        
        /// <summary>
        /// Initializes a new instance of the UTF8StringBuilder with the specified buffer
        /// </summary>
        /// <param name="buffer">The buffer to use for building the UTF-8 string</param>
        public UTF8StringBuilder(Span<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }
        
        /// <summary>
        /// Appends a single byte to the builder
        /// </summary>
        /// <param name="b">The byte to append</param>
        public void Append(byte b)
        {
            if (_position < _buffer.Length)
            {
                _buffer[_position++] = b;
            }
        }
        
        /// <summary>
        /// Appends a span of bytes to the builder
        /// </summary>
        /// <param name="bytes">The bytes to append</param>
        public void Append(ReadOnlySpan<byte> bytes)
        {
            int toCopy = Math.Min(bytes.Length, _buffer.Length - _position);
            bytes.Slice(0, toCopy).CopyTo(_buffer.Slice(_position));
            _position += toCopy;
        }
        
        /// <summary>
        /// Returns the current content as a ReadOnlySpan of bytes
        /// </summary>
        /// <returns>A ReadOnlySpan representing the UTF-8 content built so far</returns>
        public ReadOnlySpan<byte> AsSpan() => _buffer.Slice(0, _position);
        
        /// <summary>
        /// Gets the current length of the content in bytes
        /// </summary>
        public int Length => _position;
        
        /// <summary>
        /// Gets a value indicating whether the buffer is full
        /// </summary>
        public bool IsFull => _position >= _buffer.Length;
    }
}