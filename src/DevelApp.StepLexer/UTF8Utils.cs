using System;
using System.Buffers;
using System.Text;

namespace DevelApp.StepLexer
{
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
        
        public UTF8StringBuilder(Span<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }
        
        public void Append(byte b)
        {
            if (_position < _buffer.Length)
            {
                _buffer[_position++] = b;
            }
        }
        
        public void Append(ReadOnlySpan<byte> bytes)
        {
            int toCopy = Math.Min(bytes.Length, _buffer.Length - _position);
            bytes.Slice(0, toCopy).CopyTo(_buffer.Slice(_position));
            _position += toCopy;
        }
        
        public ReadOnlySpan<byte> AsSpan() => _buffer.Slice(0, _position);
        
        public int Length => _position;
        
        public bool IsFull => _position >= _buffer.Length;
    }
}