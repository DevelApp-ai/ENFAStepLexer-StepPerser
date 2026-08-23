/**
 * ParseResult class for StepParser Java bindings
 * Represents the result of parsing
 */
package ai.develapp.stepparser;

import ai.develapp.steplexer.LexerResult;
import java.util.ArrayList;
import java.util.List;

/**
 * Represents the result of parsing by the StepParser
 */
public class ParseResult {
    private final ParseNode root;
    private final LexerResult lexerResult;
    private final List<String> errors;
    private final boolean success;
    private final long parseTimeMs;

    /**
     * Creates a new ParseResult
     * @param root The root node of the parse tree
     * @param lexerResult The result from the lexer
     * @param errors The list of errors encountered during parsing
     * @param success Whether the parsing was successful
     * @param parseTimeMs The time taken to parse in milliseconds
     */
    public ParseResult(ParseNode root, LexerResult lexerResult, List<String> errors, 
                      boolean success, long parseTimeMs) {
        this.root = root;
        this.lexerResult = lexerResult;
        this.errors = new ArrayList<>(errors);
        this.success = success;
        this.parseTimeMs = parseTimeMs;
    }

    /**
     * Gets the root node of the parse tree
     * @return The root node
     */
    public ParseNode getRoot() {
        return root;
    }

    /**
     * Gets the result from the lexer
     * @return The lexer result
     */
    public LexerResult getLexerResult() {
        return lexerResult;
    }

    /**
     * Gets the list of errors encountered during parsing
     * @return The list of errors
     */
    public List<String> getErrors() {
        return errors;
    }

    /**
     * Gets whether the parsing was successful
     * @return true if successful
     */
    public boolean isSuccess() {
        return success;
    }

    /**
     * Gets the time taken to parse in milliseconds
     * @return The parse time in milliseconds
     */
    public long getParseTimeMs() {
        return parseTimeMs;
    }

    @Override
    public String toString() {
        return "ParseResult{" +
                "success=" + success +
                ", errors=" + errors +
                ", parseTimeMs=" + parseTimeMs +
                ", tokenCount=" + (lexerResult != null ? lexerResult.getTokenCount() : 0) +
                '}';
    }
}
