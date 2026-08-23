/**
 * TokenType enum for StepLexer Java bindings
 * Mirrors the C# TokenType enum from DevelApp.StepLexer
 */
package ai.develapp.steplexer;

/**
 * Enumeration of token types used in both regex pattern parsing and source code tokenization
 */
public enum TokenType {
    // Regex pattern tokens
    LITERAL_CHAR,
    CHAR_CLASS,
    NEGATED_CHAR_CLASS,
    QUANTIFIER,
    GROUP_START,
    GROUP_END,
    ALTERNATION,
    ANCHOR_START,
    ANCHOR_END,
    ANCHOR_WORD_BOUNDARY,
    ANCHOR_NON_WORD_BOUNDARY,
    
    // Quantifier types
    QUANTIFIER_STAR,
    QUANTIFIER_PLUS,
    QUANTIFIER_QUESTION,
    QUANTIFIER_EXACT,
    QUANTIFIER_MIN,
    QUANTIFIER_MIN_MAX,
    
    // Escape sequences
    ESCAPED_CHAR,
    ESCAPED_DIGIT,
    ESCAPED_WHITESPACE,
    ESCAPED_WORD_CHAR,
    ESCAPED_NON_WORD_CHAR,
    
    // Unicode support
    UNICODE_CODEPOINT,
    UNICODE_PROPERTY,
    UNICODE_CATEGORY,
    UNICODE_SCRIPT,
    
    // POSIX character classes
    POSIX_ALPHA,
    POSIX_DIGIT,
    POSIX_ALNUM,
    POSIX_SPACE,
    POSIX_WORD,
    
    // Groups and assertions
    CAPTURING_GROUP_START,
    CAPTURING_GROUP_END,
    NON_CAPTURING_GROUP_START,
    NON_CAPTURING_GROUP_END,
    LOOKAHEAD_START,
    LOOKAHEAD_END,
    LOOKBEHIND_START,
    LOOKBEHIND_END,
    
    // Source code tokens
    IDENTIFIER,
    KEYWORD,
    STRING_LITERAL,
    CHAR_LITERAL,
    NUMBER_LITERAL,
    OPERATOR,
    PUNCTUATION,
    WHITESPACE,
    COMMENT_SINGLE,
    COMMENT_MULTI,
    
    // Special tokens
    EOF,
    UNKNOWN
}
