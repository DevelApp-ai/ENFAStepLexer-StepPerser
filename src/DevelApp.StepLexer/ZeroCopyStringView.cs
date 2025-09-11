using System;
using System.Text;

namespace DevelApp.SplitLexer
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

        public int Length => _length;
        public bool IsEmpty => _length == 0;
        
        public ReadOnlySpan<byte> AsSpan() => _utf8Data.Span.Slice(_start, _length);
        
        public ZeroCopyStringView Slice(int start, int length = -1)
        {
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            int actualLength = length == -1 ? _length - start : length;
            if (actualLength < 0 || start + actualLength > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
                
            return new ZeroCopyStringView(_utf8Data, _start + start, actualLength);
        }
        
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
        /// Convert to string only when necessary (expensive operation)
        /// </summary>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(AsSpan());
        }
        
        public bool Equals(ZeroCopyStringView other)
        {
            return AsSpan().SequenceEqual(other.AsSpan());
        }
        
        public override bool Equals(object? obj)
        {
            return obj is ZeroCopyStringView other && Equals(other);
        }
        
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
        
        public static bool operator ==(ZeroCopyStringView left, ZeroCopyStringView right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(ZeroCopyStringView left, ZeroCopyStringView right)
        {
            return !left.Equals(right);
        }
    }
}