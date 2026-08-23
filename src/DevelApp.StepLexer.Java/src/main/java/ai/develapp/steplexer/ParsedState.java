/**
 * ParsedState class for StepLexer Java bindings
 * Mirrors the C# ParsedState class from DevelApp.StepLexer
 */
package ai.develapp.steplexer;

/**
 * Represents a parsed state from Phase 2 processing
 */
public class ParsedState {
    private final TokenType tokenType;
    private final String text;
    private final int position;
    private final boolean isAmbiguous;

    /**
     * Creates a new ParsedState
     * @param tokenType The token type for this parsed state
     * @param text The text content of the parsed state
     * @param position The position of the parsed state in the input
     * @param isAmbiguous Whether this parsed state is ambiguous
     */
    public ParsedState(TokenType tokenType, String text, int position, boolean isAmbiguous) {
        this.tokenType = tokenType;
        this.text = text;
        this.position = position;
        this.isAmbiguous = isAmbiguous;
    }

    /**
     * Gets the token type for this parsed state
     * @return The token type
     */
    public TokenType getTokenType() {
        return tokenType;
    }

    /**
     * Gets the text content of the parsed state
     * @return The text content
     */
    public String getText() {
        return text;
    }

    /**
     * Gets the position of the parsed state in the input
     * @return The position
     */
    public int getPosition() {
        return position;
    }

    /**
     * Gets whether this parsed state is ambiguous
     * @return true if ambiguous
     */
    public boolean isAmbiguous() {
        return isAmbiguous;
    }

    @Override
    public String toString() {
        return "ParsedState{" +
                "tokenType=" + tokenType +
                ", text='" + text + '\'' +
                ", position=" + position +
                ", isAmbiguous=" + isAmbiguous +
                '}';
    }
}
