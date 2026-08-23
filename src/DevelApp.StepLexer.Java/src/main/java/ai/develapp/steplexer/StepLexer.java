/**
 * StepLexer class for Java bindings
 * Mirrors the C# StepLexer class from DevelApp.StepLexer
 * 
 * This class provides zero-copy UTF-8 tokenization with PCRE2 support
 * for the StepParser architecture.
 */
package ai.develapp.steplexer;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Main lexer class for StepLexer Java bindings
 * Provides zero-copy UTF-8 tokenization with PCRE2 support
 */
public class StepLexer {
    private String language;
    private boolean preserveComments;
    private boolean includeWhitespace;

    /**
     * Creates a new StepLexer for the specified language
     * @param language The language to lex
     */
    public StepLexer(String language) {
        this.language = language;
        this.preserveComments = true;
        this.includeWhitespace = false;
    }

    /**
     * Creates a new StepLexer with default settings
     */
    public StepLexer() {
        this("csharp");
    }

    /**
     * Gets the language being lexed
     * @return The language
     */
    public String getLanguage() {
        return language;
    }

    /**
     * Sets the language being lexed
     * @param language The language
     */
    public void setLanguage(String language) {
        this.language = language;
    }

    /**
     * Gets whether comments are preserved
     * @return true if comments are preserved
     */
    public boolean isPreserveComments() {
        return preserveComments;
    }

    /**
     * Sets whether comments are preserved
     * @param preserveComments true to preserve comments
     */
    public void setPreserveComments(boolean preserveComments) {
        this.preserveComments = preserveComments;
    }

    /**
     * Gets whether whitespace is included
     * @return true if whitespace is included
     */
    public boolean isIncludeWhitespace() {
        return includeWhitespace;
    }

    /**
     * Sets whether whitespace is included
     * @param includeWhitespace true to include whitespace
     */
    public void setIncludeWhitespace(boolean includeWhitespace) {
        this.includeWhitespace = includeWhitespace;
    }

    /**
     * Tokenizes the input source code
     * @param sourceCode The source code to tokenize
     * @return The lexer result
     */
    public LexerResult tokenize(String sourceCode) {
        return tokenize(sourceCode, "UTF-8");
    }

    /**
     * Tokenizes the input source code with specified encoding
     * @param sourceCode The source code to tokenize
     * @param encoding The encoding of the source code
     * @return The lexer result
     */
    public LexerResult tokenize(String sourceCode, String encoding) {
        List<SplittableToken> tokens = new ArrayList<>();
        List<String> errors = new ArrayList<>();
        int position = 0;
        int ambiguousCount = 0;

        try {
            byte[] bytes = sourceCode.getBytes(encoding);
            ZeroCopyStringView view = new ZeroCopyStringView(bytes);
            
            while (position < view.getByteLength()) {
                SplittableToken token = tokenizeNext(view, position);
                if (token != null) {
                    tokens.add(token);
                    if (token.hasAlternatives()) {
                        ambiguousCount++;
                    }
                    position = token.getPosition() + token.getText().length();
                } else {
                    // Skip one byte if we can't tokenize
                    position++;
                }
            }
        } catch (Exception e) {
            errors.add("Error during tokenization: " + e.getMessage());
            return new LexerResult(tokens, errors, false, tokens.size(), ambiguousCount);
        }

        return new LexerResult(tokens, errors, true, tokens.size(), ambiguousCount);
    }

    /**
     * Tokenizes the next token from the input
     * @param view The zero-copy string view
     * @param position The current position
     * @return The next token, or null if none
     */
    private SplittableToken tokenizeNext(ZeroCopyStringView view, int position) {
        if (position >= view.getByteLength()) {
            return null;
        }

        // Try to identify the token type
        byte current = view.getByte(position);
        
        // Check for whitespace
        if (isWhitespace(current)) {
            if (includeWhitespace) {
                return readWhitespace(view, position);
            } else {
                // Skip whitespace
                int end = position;
                while (end < view.getByteLength() && isWhitespace(view.getByte(end))) {
                    end++;
                }
                return null; // Skip whitespace
            }
        }

        // Check for identifiers and keywords
        if (isIdentifierStart(current)) {
            return readIdentifier(view, position);
        }

        // Check for numbers
        if (isDigit(current)) {
            return readNumber(view, position);
        }

        // Check for strings
        if (current == '"' || current == '\'') {
            return readString(view, position);
        }

        // Check for characters
        if (current == '\'') {
            return readChar(view, position);
        }

        // Check for comments
        if (current == '/') {
            if (position + 1 < view.getByteLength()) {
                byte next = view.getByte(position + 1);
                if (next == '/') {
                    return readSingleLineComment(view, position);
                } else if (next == '*') {
                    return readMultiLineComment(view, position);
                }
            }
        }

        // Check for operators and punctuation
        if (isOperatorOrPunctuation(current)) {
            return readOperatorOrPunctuation(view, position);
        }

        // Unknown token
        return new SplittableToken(
            view.substring(position, position + 1).toString(),
            TokenType.UNKNOWN,
            position
        );
    }

    /**
     * Reads a whitespace token
     */
    private SplittableToken readWhitespace(ZeroCopyStringView view, int position) {
        int start = position;
        while (position < view.getByteLength() && isWhitespace(view.getByte(position))) {
            position++;
        }
        String text = view.substring(start, position).toString();
        return new SplittableToken(text, TokenType.WHITESPACE, start);
    }

    /**
     * Reads an identifier or keyword token
     */
    private SplittableToken readIdentifier(ZeroCopyStringView view, int position) {
        int start = position;
        while (position < view.getByteLength() && isIdentifierChar(view.getByte(position))) {
            position++;
        }
        String text = view.substring(start, position).toString();
        TokenType type = isKeyword(text) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
        return new SplittableToken(text, type, start);
    }

    /**
     * Reads a number token
     */
    private SplittableToken readNumber(ZeroCopyStringView view, int position) {
        int start = position;
        boolean isHex = false;
        boolean isFloat = false;
        
        // Check for hex prefix
        if (position + 1 < view.getByteLength() && 
            (view.getByte(position) == '0' && 
             (view.getByte(position + 1) == 'x' || view.getByte(position + 1) == 'X'))) {
            isHex = true;
            position += 2;
        }
        
        // Read digits
        while (position < view.getByteLength()) {
            byte b = view.getByte(position);
            if (isHex) {
                if (!isHexDigit(b)) break;
            } else {
                if (!isDigit(b) && b != '.') break;
                if (b == '.') isFloat = true;
            }
            position++;
        }
        
        // Check for exponent
        if (!isHex && position < view.getByteLength()) {
            byte b = view.getByte(position);
            if (b == 'e' || b == 'E') {
                position++;
                if (position < view.getByteLength()) {
                    b = view.getByte(position);
                    if (b == '+' || b == '-') position++;
                }
                while (position < view.getByteLength() && isDigit(view.getByte(position))) {
                    position++;
                }
                isFloat = true;
            }
        }
        
        String text = view.substring(start, position).toString();
        return new SplittableToken(text, TokenType.NUMBER_LITERAL, start);
    }

    /**
     * Reads a string token
     */
    private SplittableToken readString(ZeroCopyStringView view, int position) {
        int start = position;
        byte quote = view.getByte(position);
        position++;
        
        StringBuilder sb = new StringBuilder();
        boolean escaped = false;
        
        while (position < view.getByteLength()) {
            byte b = view.getByte(position);
            if (escaped) {
                // Handle escape sequences
                switch (b) {
                    case 'n': sb.append('\n'); break;
                    case 't': sb.append('\t'); break;
                    case 'r': sb.append('\r'); break;
                    case 'b': sb.append('\b'); break;
                    case 'f': sb.append('\f'); break;
                    case '\\': sb.append('\\'); break;
                    case '"': sb.append('"'); break;
                    case '\'': sb.append('\''); break;
                    default: sb.append((char)b); break;
                }
                escaped = false;
            } else if (b == '\\') {
                escaped = true;
            } else if (b == quote) {
                position++;
                String text = sb.toString();
                return new SplittableToken(text, TokenType.STRING_LITERAL, start);
            } else {
                sb.append((char)b);
            }
            position++;
        }
        
        // Unterminated string
        return new SplittableToken(sb.toString(), TokenType.STRING_LITERAL, start);
    }

    /**
     * Reads a character token
     */
    private SplittableToken readChar(ZeroCopyStringView view, int position) {
        int start = position;
        position++;
        
        byte b = view.getByte(position);
        if (b == '\\') {
            position++;
            b = view.getByte(position);
        }
        
        if (view.getByte(position) == '\'') {
            position++;
            return new SplittableToken(String.valueOf((char)b), TokenType.CHAR_LITERAL, start);
        }
        
        // Unterminated char
        return new SplittableToken(String.valueOf((char)b), TokenType.CHAR_LITERAL, start);
    }

    /**
     * Reads a single-line comment
     */
    private SplittableToken readSingleLineComment(ZeroCopyStringView view, int position) {
        int start = position;
        position += 2; // Skip //
        
        while (position < view.getByteLength()) {
            byte b = view.getByte(position);
            if (b == '\n') break;
            position++;
        }
        
        if (!preserveComments) {
            return null;
        }
        
        String text = view.substring(start, position).toString();
        return new SplittableToken(text, TokenType.COMMENT_SINGLE, start);
    }

    /**
     * Reads a multi-line comment
     */
    private SplittableToken readMultiLineComment(ZeroCopyStringView view, int position) {
        int start = position;
        position += 2; // Skip /*
        
        while (position + 1 < view.getByteLength()) {
            byte b1 = view.getByte(position);
            byte b2 = view.getByte(position + 1);
            if (b1 == '*' && b2 == '/') {
                position += 2;
                break;
            }
            position++;
        }
        
        if (!preserveComments) {
            return null;
        }
        
        String text = view.substring(start, position).toString();
        return new SplittableToken(text, TokenType.COMMENT_MULTI, start);
    }

    /**
     * Reads an operator or punctuation token
     */
    private SplittableToken readOperatorOrPunctuation(ZeroCopyStringView view, int position) {
        int start = position;
        
        // Check for multi-character operators
        if (position + 1 < view.getByteLength()) {
            String twoChars = view.substring(position, position + 2).toString();
            if (isMultiCharOperator(twoChars)) {
                return new SplittableToken(twoChars, TokenType.OPERATOR, start);
            }
        }
        
        // Single character operator/punctuation
        byte b = view.getByte(position);
        return new SplittableToken(String.valueOf((char)b), TokenType.OPERATOR, start);
    }

    // Helper methods
    
    private boolean isWhitespace(byte b) {
        return b == ' ' || b == '\t' || b == '\n' || b == '\r' || b == '\f';
    }

    private boolean isIdentifierStart(byte b) {
        return (b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || b == '_' || b == '$';
    }

    private boolean isIdentifierChar(byte b) {
        return isIdentifierStart(b) || isDigit(b);
    }

    private boolean isDigit(byte b) {
        return b >= '0' && b <= '9';
    }

    private boolean isHexDigit(byte b) {
        return isDigit(b) || (b >= 'a' && b <= 'f') || (b >= 'A' && b <= 'F');
    }

    private boolean isOperatorOrPunctuation(byte b) {
        return "+-*/%=<>!&|^~.,;:()[]{}?@#".indexOf(b) >= 0;
    }

    private boolean isMultiCharOperator(String s) {
        return s.equals("==") || s.equals("!=") || s.equals("<=") || s.equals(">=") ||
               s.equals("&&") || s.equals("||") || s.equals("++") || s.equals("--") ||
               s.equals("+=") || s.equals("-=") || s.equals("*=") || s.equals("/=") ||
               s.equals("===") || s.equals("!==") || s.equals("=>") || s.equals("//") ||
               s.equals("/*") || s.equals("*/");
    }

    private boolean isKeyword(String text) {
        switch (language) {
            case "csharp":
                return isCSharpKeyword(text);
            case "java":
                return isJavaKeyword(text);
            case "javascript":
                return isJavaScriptKeyword(text);
            default:
                return isCSharpKeyword(text);
        }
    }

    private boolean isCSharpKeyword(String text) {
        String[] keywords = {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate",
            "do", "double", "else", "enum", "event", "explicit", "extern", "false",
            "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
            "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
            "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte",
            "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct",
            "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };
        for (String keyword : keywords) {
            if (keyword.equals(text)) return true;
        }
        return false;
    }

    private boolean isJavaKeyword(String text) {
        String[] keywords = {
            "abstract", "assert", "boolean", "break", "byte", "case", "catch", "char",
            "class", "const", "continue", "default", "do", "double", "else", "enum",
            "extends", "final", "finally", "float", "for", "goto", "if", "implements",
            "import", "instanceof", "int", "interface", "long", "native", "new",
            "package", "private", "protected", "public", "return", "short", "static",
            "strictfp", "super", "switch", "synchronized", "this", "throw", "throws",
            "transient", "try", "void", "volatile", "while"
        };
        for (String keyword : keywords) {
            if (keyword.equals(text)) return true;
        }
        return false;
    }

    private boolean isJavaScriptKeyword(String text) {
        String[] keywords = {
            "break", "case", "catch", "class", "const", "continue", "debugger", "default",
            "delete", "do", "else", "export", "extends", "finally", "for", "function",
            "if", "import", "in", "instanceof", "new", "return", "super", "switch",
            "this", "throw", "try", "typeof", "var", "void", "while", "with", "yield"
        };
        for (String keyword : keywords) {
            if (keyword.equals(text)) return true;
        }
        return false;
    }
}
