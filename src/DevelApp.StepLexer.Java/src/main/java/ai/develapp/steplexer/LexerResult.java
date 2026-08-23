/**
 * LexerResult class for StepLexer Java bindings
 * Represents the result of lexical analysis
 */
package ai.develapp.steplexer;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents the result of lexical analysis by the StepLexer
 */
public class LexerResult {
    private final List<SplittableToken> tokens;
    private final List<String> errors;
    private final boolean success;
    private final int tokenCount;
    private final int ambiguousTokenCount;

    /**
     * Creates a new LexerResult
     * @param tokens The list of tokens produced by the lexer
     * @param errors The list of errors encountered during lexical analysis
     * @param success Whether the lexical analysis was successful
     * @param tokenCount The total number of tokens
     * @param ambiguousTokenCount The number of ambiguous tokens
     */
    public LexerResult(List<SplittableToken> tokens, List<String> errors, 
                      boolean success, int tokenCount, int ambiguousTokenCount) {
        this.tokens = new ArrayList<>(tokens);
        this.errors = new ArrayList<>(errors);
        this.success = success;
        this.tokenCount = tokenCount;
        this.ambiguousTokenCount = ambiguousTokenCount;
    }

    /**
     * Gets the list of tokens produced by the lexer
     * @return The list of tokens
     */
    public List<SplittableToken> getTokens() {
        return tokens;
    }

    /**
     * Gets the list of errors encountered during lexical analysis
     * @return The list of errors
     */
    public List<String> getErrors() {
        return errors;
    }

    /**
     * Gets whether the lexical analysis was successful
     * @return true if successful
     */
    public boolean isSuccess() {
        return success;
    }

    /**
     * Gets the total number of tokens
     * @return The token count
     */
    public int getTokenCount() {
        return tokenCount;
    }

    /**
     * Gets the number of ambiguous tokens
     * @return The ambiguous token count
     */
    public int getAmbiguousTokenCount() {
        return ambiguousTokenCount;
    }

    @Override
    public String toString() {
        return "LexerResult{" +
                "tokenCount=" + tokenCount +
                ", ambiguousTokenCount=" + ambiguousTokenCount +
                ", success=" + success +
                ", errors=" + errors +
                '}';
    }
}
