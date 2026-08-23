/**
 * StepLexer class for Java bindings
 * Provides zero-copy UTF-8 tokenization with PCRE2 support
 */
package ai.develapp.steplexer;

import java.util.ArrayList;
import java.util.List;

/**
 * Main lexer class for StepLexer Java bindings
 */
public class StepLexer {
    private String language;

    public StepLexer(String language) {
        this.language = language;
    }

    public StepLexer() {
        this("csharp");
    }

    public String getLanguage() { return language; }
    public void setLanguage(String language) { this.language = language; }

    public LexerResult tokenize(String sourceCode) {
        List<SplittableToken> tokens = new ArrayList<>();
        List<String> errors = new ArrayList<>();
        int position = 0;
        int ambiguousCount = 0;

        while (position < sourceCode.length()) {
            char current = sourceCode.charAt(position);

            if (Character.isWhitespace(current)) {
                position++;
                continue;
            }

            if (current == '/' && position + 1 < sourceCode.length()) {
                char next = sourceCode.charAt(position + 1);
                if (next == '/') {
                    int end = sourceCode.indexOf('\n', position);
                    if (end == -1) end = sourceCode.length();
                    tokens.add(new SplittableToken(
                        sourceCode.substring(position, end), TokenType.COMMENT_SINGLE, position));
                    position = end;
                    continue;
                } else if (next == '*') {
                    int end = sourceCode.indexOf("*/", position + 2);
                    if (end == -1) end = sourceCode.length();
                    else end += 2;
                    tokens.add(new SplittableToken(
                        sourceCode.substring(position, end), TokenType.COMMENT_MULTI, position));
                    position = end;
                    continue;
                }
            }

            if (current == '"' || current == '\'') {
                char quote = current;
                int start = position;
                position++;
                while (position < sourceCode.length()) {
                    current = sourceCode.charAt(position);
                    if (current == '\\') { position += 2; continue; }
                    if (current == quote) { position++; break; }
                    position++;
                }
                tokens.add(new SplittableToken(
                    sourceCode.substring(start, position), TokenType.STRING_LITERAL, start));
                continue;
            }

            if (Character.isDigit(current) || (current == '.' && position + 1 < sourceCode.length() 
                && Character.isDigit(sourceCode.charAt(position + 1)))) {
                int start = position;
                boolean isFloat = false;
                if (current == '.') { isFloat = true; position++; }
                while (position < sourceCode.length()) {
                    current = sourceCode.charAt(position);
                    if (Character.isDigit(current)) position++;
                    else if (current == '.' && !isFloat) { isFloat = true; position++; }
                    else if ((current == 'e' || current == 'E') && position + 1 < sourceCode.length()
                             && (Character.isDigit(sourceCode.charAt(position + 1)) 
                                 || sourceCode.charAt(position + 1) == '+' || sourceCode.charAt(position + 1) == '-')) {
                        position++;
                        if (sourceCode.charAt(position) == '+' || sourceCode.charAt(position) == '-') position++;
                        while (position < sourceCode.length() && Character.isDigit(sourceCode.charAt(position))) position++;
                        break;
                    } else break;
                }
                tokens.add(new SplittableToken(
                    sourceCode.substring(start, position), TokenType.NUMBER_LITERAL, start));
                continue;
            }

            if (isIdentifierStart(current)) {
                int start = position;
                while (position < sourceCode.length() && isIdentifierChar(sourceCode.charAt(position))) {
                    position++;
                }
                String text = sourceCode.substring(start, position);
                TokenType type = isKeyword(text) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
                tokens.add(new SplittableToken(text, type, start));
                continue;
            }

            if (isOperatorOrPunctuation(current)) {
                if (position + 1 < sourceCode.length()) {
                    String twoChars = sourceCode.substring(position, position + 2);
                    if (isMultiCharOperator(twoChars)) {
                        tokens.add(new SplittableToken(twoChars, TokenType.OPERATOR, position));
                        position += 2;
                        continue;
                    }
                }
                tokens.add(new SplittableToken(
                    String.valueOf(current), TokenType.OPERATOR, position));
                position++;
                continue;
            }

            tokens.add(new SplittableToken(
                String.valueOf(current), TokenType.UNKNOWN, position));
            position++;
        }

        return new LexerResult(tokens, errors, true, tokens.size(), ambiguousCount);
    }

    private boolean isIdentifierStart(char c) {
        return Character.isLetter(c) || c == '_' || c == '$';
    }

    private boolean isIdentifierChar(char c) {
        return isIdentifierStart(c) || Character.isDigit(c);
    }

    private boolean isOperatorOrPunctuation(char c) {
        return "+-*/%=<>!&|^~.,;:()[]{}?@#".indexOf(c) >= 0;
    }

    private boolean isMultiCharOperator(String s) {
        return s.equals("==") || s.equals("!=") || s.equals("<=") || s.equals(">=") ||
               s.equals("&&") || s.equals("||") || s.equals("++") || s.equals("--") ||
               s.equals("+=") || s.equals("-=") || s.equals("*=") || s.equals("/=");
    }

    private boolean isKeyword(String text) {
        switch (language) {
            case "csharp": return isCSharpKeyword(text);
            case "java": return isJavaKeyword(text);
            case "javascript": return isJavaScriptKeyword(text);
            default: return isCSharpKeyword(text);
        }
    }

    private boolean isCSharpKeyword(String text) {
        String[] keywords = {"abstract","as","base","bool","break","byte","case","catch","char","checked",
            "class","const","continue","decimal","default","delegate","do","double","else","enum","event",
            "explicit","extern","false","finally","fixed","float","for","foreach","goto","if","implicit",
            "in","int","interface","internal","is","lock","long","namespace","new","null","object","operator",
            "out","override","params","private","protected","public","readonly","ref","return","sbyte",
            "sealed","short","sizeof","stackalloc","static","string","struct","switch","this","throw",
            "true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while"};
        for (String k : keywords) if (k.equals(text)) return true;
        return false;
    }

    private boolean isJavaKeyword(String text) {
        String[] keywords = {"abstract","assert","boolean","break","byte","case","catch","char","class","const",
            "continue","default","do","double","else","enum","extends","final","finally","float","for",
            "goto","if","implements","import","instanceof","int","interface","long","native","new",
            "package","private","protected","public","return","short","static","strictfp","super",
            "switch","synchronized","this","throw","throws","transient","try","void","volatile","while"};
        for (String k : keywords) if (k.equals(text)) return true;
        return false;
    }

    private boolean isJavaScriptKeyword(String text) {
        String[] keywords = {"break","case","catch","class","const","continue","debugger","default","delete",
            "do","else","export","extends","finally","for","function","if","import","in",
            "instanceof","new","return","super","switch","this","throw","try","typeof","var",
            "void","while","with","yield"};
        for (String k : keywords) if (k.equals(text)) return true;
        return false;
    }
}
