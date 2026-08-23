/**
 * ParseResult class for StepParser Java bindings
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

    public ParseResult(ParseNode root, LexerResult lexerResult, List<String> errors,
                      boolean success, long parseTimeMs) {
        this.root = root;
        this.lexerResult = lexerResult;
        this.errors = new ArrayList<>(errors);
        this.success = success;
        this.parseTimeMs = parseTimeMs;
    }

    public ParseNode getRoot() { return root; }
    public LexerResult getLexerResult() { return lexerResult; }
    public List<String> getErrors() { return errors; }
    public boolean isSuccess() { return success; }
    public long getParseTimeMs() { return parseTimeMs; }
}
