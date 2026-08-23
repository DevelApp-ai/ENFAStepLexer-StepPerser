/**
 * ZeroCopyStringView class for StepLexer Java bindings
 * Provides memory-efficient string operations
 */
package ai.develapp.steplexer;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

/**
 * Memory-efficient string view that avoids copying byte arrays
 */
public class ZeroCopyStringView {
    private final ByteBuffer buffer;
    private final int offset;
    private final int length;

    public ZeroCopyStringView(byte[] bytes) {
        this(ByteBuffer.wrap(bytes), 0, bytes.length);
    }

    public ZeroCopyStringView(byte[] bytes, int offset, int length) {
        this(ByteBuffer.wrap(bytes), offset, length);
    }

    public ZeroCopyStringView(ByteBuffer buffer, int offset, int length) {
        this.buffer = buffer.slice();
        this.offset = offset;
        this.length = length;
        this.buffer.position(offset);
        this.buffer.limit(offset + length);
    }

    public ZeroCopyStringView(String str) {
        this(str.getBytes(StandardCharsets.UTF_8));
    }

    public byte getByte(int index) {
        return buffer.get(offset + index);
    }

    public int getByteLength() {
        return length;
    }

    public ZeroCopyStringView substring(int start, int end) {
        return new ZeroCopyStringView(buffer, offset + start, end - start);
    }

    public ZeroCopyStringView substring(int start) {
        return substring(start, length);
    }

    @Override
    public String toString() {
        byte[] bytes = new byte[length];
        buffer.position(offset);
        buffer.get(bytes);
        return new String(bytes, StandardCharsets.UTF_8);
    }

    public boolean startsWith(String prefix) {
        byte[] prefixBytes = prefix.getBytes(StandardCharsets.UTF_8);
        if (prefixBytes.length > length) return false;
        for (int i = 0; i < prefixBytes.length; i++) {
            if (getByte(i) != prefixBytes[i]) return false;
        }
        return true;
    }

    public boolean isEmpty() {
        return length == 0;
    }

    public byte[] toByteArray() {
        byte[] bytes = new byte[length];
        buffer.position(offset);
        buffer.get(bytes);
        return bytes;
    }
}
