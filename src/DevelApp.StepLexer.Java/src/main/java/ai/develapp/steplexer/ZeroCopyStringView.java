/**
 * ZeroCopyStringView class for StepLexer Java bindings
 * Provides memory-efficient string operations similar to C# ZeroCopyStringView
 */
package ai.develapp.steplexer;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

/**
 * Memory-efficient string view that avoids copying byte arrays
 * Similar to the C# ZeroCopyStringView implementation
 */
public class ZeroCopyStringView {
    private final ByteBuffer buffer;
    private final int offset;
    private final int length;

    /**
     * Creates a ZeroCopyStringView from a byte array
     * @param bytes The byte array containing UTF-8 encoded text
     */
    public ZeroCopyStringView(byte[] bytes) {
        this(ByteBuffer.wrap(bytes), 0, bytes.length);
    }

    /**
     * Creates a ZeroCopyStringView from a byte array with offset and length
     * @param bytes The byte array containing UTF-8 encoded text
     * @param offset The starting offset in the byte array
     * @param length The length of the string in bytes
     */
    public ZeroCopyStringView(byte[] bytes, int offset, int length) {
        this(ByteBuffer.wrap(bytes), offset, length);
    }

    /**
     * Creates a ZeroCopyStringView from a ByteBuffer
     * @param buffer The ByteBuffer containing UTF-8 encoded text
     * @param offset The starting offset in the buffer
     * @param length The length of the string in bytes
     */
    public ZeroCopyStringView(ByteBuffer buffer, int offset, int length) {
        this.buffer = buffer.slice();
        this.offset = offset;
        this.length = length;
        this.buffer.position(offset);
        this.buffer.limit(offset + length);
    }

    /**
     * Creates a ZeroCopyStringView from a String
     * Note: This creates a copy since Java Strings are UTF-16
     * @param str The string to view
     */
    public ZeroCopyStringView(String str) {
        this(str.getBytes(StandardCharsets.UTF_8));
    }

    /**
     * Gets the byte at the specified index
     * @param index The index
     * @return The byte at the specified index
     */
    public byte getByte(int index) {
        if (index < 0 || index >= length) {
            throw new IndexOutOfBoundsException("Index out of bounds: " + index);
        }
        return buffer.get(offset + index);
    }

    /**
     * Gets the length of this string view in bytes
     * @return The length in bytes
     */
    public int getByteLength() {
        return length;
    }

    /**
     * Gets the length of this string view in characters
     * @return The length in characters
     */
    public int getCharLength() {
        // Count UTF-8 characters (not bytes)
        int count = 0;
        int i = 0;
        while (i < length) {
            byte b = getByte(i);
            if ((b & 0x80) == 0) {
                // 1-byte character
                i++;
            } else if ((b & 0xE0) == 0xC0) {
                // 2-byte character
                i += 2;
            } else if ((b & 0xF0) == 0xE0) {
                // 3-byte character
                i += 3;
            } else if ((b & 0xF8) == 0xF0) {
                // 4-byte character
                i += 4;
            }
            count++;
        }
        return count;
    }

    /**
     * Gets a substring as a new ZeroCopyStringView
     * @param start The starting index (in bytes)
     * @param end The ending index (exclusive, in bytes)
     * @return A new ZeroCopyStringView for the substring
     */
    public ZeroCopyStringView substring(int start, int end) {
        if (start < 0 || end > length || start > end) {
            throw new IndexOutOfBoundsException("Invalid substring range: [" + start + ", " + end + ")");
        }
        return new ZeroCopyStringView(buffer, offset + start, end - start);
    }

    /**
     * Gets a substring as a new ZeroCopyStringView
     * @param start The starting index (in bytes)
     * @return A new ZeroCopyStringView for the substring
     */
    public ZeroCopyStringView substring(int start) {
        return substring(start, length);
    }

    /**
     * Converts this ZeroCopyStringView to a String
     * Note: This creates a copy
     * @return The String representation
     */
    public String toString() {
        byte[] bytes = new byte[length];
        buffer.position(offset);
        buffer.get(bytes);
        return new String(bytes, StandardCharsets.UTF_8);
    }

    /**
     * Checks if this ZeroCopyStringView starts with the specified prefix
     * @param prefix The prefix to check
     * @return true if this string starts with the prefix
     */
    public boolean startsWith(String prefix) {
        byte[] prefixBytes = prefix.getBytes(StandardCharsets.UTF_8);
        if (prefixBytes.length > length) {
            return false;
        }
        for (int i = 0; i < prefixBytes.length; i++) {
            if (getByte(i) != prefixBytes[i]) {
                return false;
            }
        }
        return true;
    }

    /**
     * Checks if this ZeroCopyStringView ends with the specified suffix
     * @param suffix The suffix to check
     * @return true if this string ends with the suffix
     */
    public boolean endsWith(String suffix) {
        byte[] suffixBytes = suffix.getBytes(StandardCharsets.UTF_8);
        if (suffixBytes.length > length) {
            return false;
        }
        for (int i = 0; i < suffixBytes.length; i++) {
            if (getByte(length - suffixBytes.length + i) != suffixBytes[i]) {
                return false;
            }
        }
        return true;
    }

    /**
     * Checks if this ZeroCopyStringView is empty
     * @return true if this string is empty
     */
    public boolean isEmpty() {
        return length == 0;
    }

    /**
     * Gets the underlying byte array (for interop with native code)
     * @return The byte array
     */
    public byte[] toByteArray() {
        byte[] bytes = new byte[length];
        buffer.position(offset);
        buffer.get(bytes);
        return bytes;
    }

    @Override
    public boolean equals(Object obj) {
        if (this == obj) return true;
        if (obj == null || getClass() != obj.getClass()) return false;
        ZeroCopyStringView that = (ZeroCopyStringView) obj;
        if (length != that.length) return false;
        for (int i = 0; i < length; i++) {
            if (getByte(i) != that.getByte(i)) return false;
        }
        return true;
    }

    @Override
    public int hashCode() {
        int result = 17;
        for (int i = 0; i < length; i++) {
            result = 31 * result + getByte(i);
        }
        return result;
    }
}
