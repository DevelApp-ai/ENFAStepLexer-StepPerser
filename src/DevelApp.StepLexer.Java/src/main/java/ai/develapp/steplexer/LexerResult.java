/**
 * LexerResult class for StepLexer Java bindings
 */
package ai.develapp.steplexer;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents the result of lexical analysis
 */
public class LexerResult {
    private final List<SplittableToken> tokens;
    private final List<String> errors;
    private final boolean success;
    private final int tokenCount;
    private final int ambiguousTokenCount;

    public LexerResult(List<SplittableToken> tokens, List<String> errors,
                      boolean success, int tokenCount, int ambiguousTokenCount) {
        this.tokens = new ArrayList<>(tokens);
        this.errors = new ArrayList<>(errors);
        this.success = success;
        this.tokenCount = tokenCount;
        this.ambiguousTokenCount = ambiguousTokenCount;
    }

    public List<SplittableToken> getTokens() { return tokens; }
    public List<String> getErrors() { return errors; }
    public boolean isSuccess() { return success; }
    public int getTokenCount() { return tokenCount; }
    public int getAmbiguousTokenCount() { return ambiguousTokenCount; }
}
