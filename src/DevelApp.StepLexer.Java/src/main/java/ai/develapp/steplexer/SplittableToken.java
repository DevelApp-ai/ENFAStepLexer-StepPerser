/**
 * SplittableToken class for StepLexer Java bindings
 * Mirrors the C# SplittableToken class from DevelApp.StepLexer
 */
package ai.develapp.steplexer;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a token that can be split into multiple alternatives during ambiguity resolution
 * Used for both regex pattern parsing and source code tokenization
 */
public class SplittableToken {
    private final String text;
    private TokenType type;
    private final int position;
    private List<SplittableToken> alternatives;

    /**
     * Initializes a new instance of the SplittableToken class
     * @param text The text content of the token
     * @param type The type of the token
     * @param position The position of the token in the input
     */
    public SplittableToken(String text, TokenType type, int position) {
        this.text = text;
        this.type = type;
        this.position = position;
        this.alternatives = new ArrayList<>();
    }

    /**
     * Gets the text content of the token
     * @return The text content
     */
    public String getText() {
        return text;
    }

    /**
     * Gets the type of the token
     * @return The token type
     */
    public TokenType getType() {
        return type;
    }

    /**
     * Sets the type of the token
     * @param type The new token type
     */
    public void setType(TokenType type) {
        this.type = type;
    }

    /**
     * Gets the position of the token in the input
     * @return The position
     */
    public int getPosition() {
        return position;
    }

    /**
     * Gets the list of alternative tokens when ambiguity is detected
     * @return The list of alternatives
     */
    public List<SplittableToken> getAlternatives() {
        return alternatives;
    }

    /**
     * Sets the list of alternative tokens
     * @param alternatives The list of alternatives
     */
    public void setAlternatives(List<SplittableToken> alternatives) {
        this.alternatives = alternatives;
    }

    /**
     * Split this token into multiple alternatives when ambiguity is detected
     * @param alternativePairs Array of alternative token text and type pairs
     */
    public void split(SplittableToken... alternativePairs) {
        for (SplittableToken token : alternativePairs) {
            alternatives.add(token);
        }
    }

    /**
     * Gets a value indicating whether this token has alternatives
     * @return true if this token has alternatives
     */
    public boolean hasAlternatives() {
        return alternatives != null && !alternatives.isEmpty();
    }

    @Override
    public String toString() {
        return "SplittableToken{" +
                "text='" + text + '\'' +
                ", type=" + type +
                ", position=" + position +
                ", hasAlternatives=" + hasAlternatives() +
                '}';
    }
}
