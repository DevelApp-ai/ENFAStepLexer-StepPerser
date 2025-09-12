using System;
using System.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Zero-copy string view implementation inspired by Cap'n Proto design patterns.
    /// Provides efficient string slicing without memory allocation.
    /// </summary>
    public readonly struct ZeroCopyStringView : IEquatable<ZeroCopyStringView>
    {
        private readonly ReadOnlyMemory<byte> _utf8Data;
        private readonly int _start;
        private readonly int _length;

        /// <summary>
        /// Initializes a new instance of the ZeroCopyStringView struct with the specified UTF-8 data
        /// </summary>
        /// <param name="utf8Data">The UTF-8 encoded byte data</param>
        /// <param name="start">The starting position within the data (default is 0)</param>
        /// <param name="length">The length of the view (-1 to use remaining length from start)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when start or length parameters are out of range</exception>
        public ZeroCopyStringView(ReadOnlyMemory<byte> utf8Data, int start = 0, int length = -1)
        {
            _utf8Data = utf8Data;
            _start = start;
            _length = length == -1 ? utf8Data.Length - start : length;
            
            if (_start < 0 || _start > utf8Data.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (_length < 0 || _start + _length > utf8Data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
        }

        /// <summary>
        /// Gets the length of the string view in bytes
        /// </summary>
        public int Length => _length;
        
        /// <summary>
        /// Gets a value indicating whether the string view is empty
        /// </summary>
        public bool IsEmpty => _length == 0;
        
        /// <summary>
        /// Returns the underlying data as a ReadOnlySpan of bytes
        /// </summary>
        /// <returns>A ReadOnlySpan representing the UTF-8 data</returns>
        public ReadOnlySpan<byte> AsSpan() => _utf8Data.Span.Slice(_start, _length);
        
        /// <summary>
        /// Creates a slice of the current string view
        /// </summary>
        /// <param name="start">The starting position for the slice</param>
        /// <param name="length">The length of the slice (-1 to use remaining length)</param>
        /// <returns>A new ZeroCopyStringView representing the slice</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when start or length parameters are out of range</exception>
        public ZeroCopyStringView Slice(int start, int length = -1)
        {
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            int actualLength = length == -1 ? _length - start : length;
            if (actualLength < 0 || start + actualLength > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
                
            return new ZeroCopyStringView(_utf8Data, _start + start, actualLength);
        }
        
        /// <summary>
        /// Gets the byte at the specified index
        /// </summary>
        /// <param name="index">The zero-based index of the byte to get</param>
        /// <returns>The byte at the specified index</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when index is out of range</exception>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException();
                return _utf8Data.Span[_start + index];
            }
        }
        
        /// <summary>
        /// Converts the string view to a string representation (expensive operation - avoid when possible)
        /// </summary>
        /// <returns>A string representation of the UTF-8 data</returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(AsSpan());
        }
        
        /// <summary>
        /// Determines whether the current string view is equal to another string view
        /// </summary>
        /// <param name="other">The other string view to compare</param>
        /// <returns>True if the string views are equal, false otherwise</returns>
        public bool Equals(ZeroCopyStringView other)
        {
            return AsSpan().SequenceEqual(other.AsSpan());
        }
        
        /// <summary>
        /// Determines whether the current string view is equal to the specified object
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the object is a ZeroCopyStringView and is equal to this instance, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            return obj is ZeroCopyStringView other && Equals(other);
        }
        
        /// <summary>
        /// Gets the hash code for the string view
        /// </summary>
        /// <returns>A hash code for the current string view</returns>
        public override int GetHashCode()
        {
            var span = AsSpan();
            var hash = new HashCode();
            foreach (byte b in span)
            {
                hash.Add(b);
            }
            return hash.ToHashCode();
        }
        
        /// <summary>
        /// Determines whether two string views are equal
        /// </summary>
        /// <param name="left">The first string view to compare</param>
        /// <param name="right">The second string view to compare</param>
        /// <returns>True if the string views are equal, false otherwise</returns>
        public static bool operator ==(ZeroCopyStringView left, ZeroCopyStringView right)
        {
            return left.Equals(right);
        }
        
        /// <summary>
        /// Determines whether two string views are not equal
        /// </summary>
        /// <param name="left">The first string view to compare</param>
        /// <param name="right">The second string view to compare</param>
        /// <returns>True if the string views are not equal, false otherwise</returns>
        public static bool operator !=(ZeroCopyStringView left, ZeroCopyStringView right)
        {
            return !left.Equals(right);
        }
    }
}