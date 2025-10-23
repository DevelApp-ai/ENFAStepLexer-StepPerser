using System;
using System.Buffers;
using System.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Encoding converter that uses System.Text.Encoding library for converting various formats to UTF-8.
    /// Supports hundreds of encodings through the .NET encoding infrastructure.
    /// </summary>
    public static class EncodingConverter
    {
        static EncodingConverter()
        {
            // Register the code pages provider to enable support for hundreds of encodings
            // This includes Windows code pages, ISO encodings, and many others
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Convert from any encoding to UTF-8 using encoding name or code page
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="sourceEncoding">The source encoding (e.g., "UTF-16", "ISO-8859-1", "Windows-1252", etc.)</param>
        /// <returns>UTF-8 encoded bytes</returns>
        /// <exception cref="ArgumentException">If the encoding is not supported</exception>
        public static byte[] ConvertToUTF8(ReadOnlySpan<byte> sourceBytes, Encoding sourceEncoding)
        {
            if (sourceBytes.IsEmpty)
                return Array.Empty<byte>();

            // If already UTF-8, just return a copy
            if (sourceEncoding.CodePage == Encoding.UTF8.CodePage)
                return sourceBytes.ToArray();

            // Use the .NET library to convert: source encoding -> string -> UTF-8
            var chars = sourceEncoding.GetString(sourceBytes.ToArray());
            return Encoding.UTF8.GetBytes(chars);
        }

        /// <summary>
        /// Convert from any encoding to UTF-8 using encoding name
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="encodingName">Name of the source encoding (e.g., "UTF-16", "ISO-8859-1", "shift_jis", etc.)</param>
        /// <returns>UTF-8 encoded bytes</returns>
        /// <exception cref="ArgumentException">If the encoding name is not recognized</exception>
        public static byte[] ConvertToUTF8(ReadOnlySpan<byte> sourceBytes, string encodingName)
        {
            if (sourceBytes.IsEmpty)
                return Array.Empty<byte>();

            var encoding = Encoding.GetEncoding(encodingName);
            return ConvertToUTF8(sourceBytes, encoding);
        }

        /// <summary>
        /// Convert from any encoding to UTF-8 using code page number
        /// </summary>
        /// <param name="sourceBytes">Source bytes in the specified encoding</param>
        /// <param name="codePage">Code page number (e.g., 1252 for Windows-1252, 28591 for ISO-8859-1)</param>
        /// <returns>UTF-8 encoded bytes</returns>
        /// <exception cref="ArgumentException">If the code page is not supported</exception>
        public static byte[] ConvertToUTF8(ReadOnlySpan<byte> sourceBytes, int codePage)
        {
            if (sourceBytes.IsEmpty)
                return Array.Empty<byte>();

            var encoding = Encoding.GetEncoding(codePage);
            return ConvertToUTF8(sourceBytes, encoding);
        }

        /// <summary>
        /// Auto-detect encoding from BOM and convert to UTF-8
        /// </summary>
        /// <param name="sourceBytes">Source bytes with potential BOM</param>
        /// <returns>UTF-8 encoded bytes (BOM removed if present)</returns>
        public static byte[] ConvertToUTF8WithAutoDetect(ReadOnlySpan<byte> sourceBytes)
        {
            if (sourceBytes.IsEmpty)
                return Array.Empty<byte>();

            var (encoding, bomLength) = DetectEncodingFromBOM(sourceBytes);
            
            // Skip BOM if detected
            var dataBytes = sourceBytes.Slice(bomLength);
            
            return ConvertToUTF8(dataBytes, encoding);
        }

        /// <summary>
        /// Detect encoding from BOM (Byte Order Mark)
        /// </summary>
        /// <param name="bytes">Bytes to analyze</param>
        /// <returns>Detected encoding and BOM length in bytes</returns>
        public static (Encoding encoding, int bomLength) DetectEncodingFromBOM(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 0)
                return (Encoding.UTF8, 0);

            // UTF-8 BOM: EF BB BF
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return (Encoding.UTF8, 3);
            
            // UTF-32 LE BOM: FF FE 00 00 (must check before UTF-16 LE)
            if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
                return (Encoding.UTF32, 4);
            
            // UTF-16 LE BOM: FF FE
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return (Encoding.Unicode, 2);
            
            // UTF-16 BE BOM: FE FF
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return (Encoding.BigEndianUnicode, 2);
            
            // UTF-32 BE BOM: 00 00 FE FF
            if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            {
                // UTF-32 BE - need to get this encoding by code page
                return (Encoding.GetEncoding(12001), 4);
            }

            // No BOM detected, assume UTF-8
            return (Encoding.UTF8, 0);
        }

        /// <summary>
        /// Get a list of all available encodings on the current system
        /// </summary>
        /// <returns>Array of encoding information</returns>
        public static EncodingInfo[] GetAvailableEncodings()
        {
            return Encoding.GetEncodings();
        }

        /// <summary>
        /// Check if an encoding is available by name
        /// </summary>
        /// <param name="encodingName">Name of the encoding</param>
        /// <returns>True if the encoding is available</returns>
        public static bool IsEncodingAvailable(string encodingName)
        {
            try
            {
                Encoding.GetEncoding(encodingName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if an encoding is available by code page
        /// </summary>
        /// <param name="codePage">Code page number</param>
        /// <returns>True if the encoding is available</returns>
        public static bool IsEncodingAvailable(int codePage)
        {
            try
            {
                Encoding.GetEncoding(codePage);
                return true;
            }
            catch
            {
                return false;
            }
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