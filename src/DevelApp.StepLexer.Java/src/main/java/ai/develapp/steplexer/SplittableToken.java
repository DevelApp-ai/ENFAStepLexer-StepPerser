/**
 * SplittableToken class for StepLexer Java bindings
 */
package ai.develapp.steplexer;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a token that can be split into multiple alternatives during ambiguity resolution
 */
public class SplittableToken {
    private final String text;
    private TokenType type;
    private final int position;
    private List<SplittableToken> alternatives;

    public SplittableToken(String text, TokenType type, int position) {
        this.text = text;
        this.type = type;
        this.position = position;
        this.alternatives = new ArrayList<>();
    }

    public String getText() { return text; }
    public TokenType getType() { return type; }
    public void setType(TokenType type) { this.type = type; }
    public int getPosition() { return position; }
    public List<SplittableToken> getAlternatives() { return alternatives; }
    public void setAlternatives(List<SplittableToken> alternatives) { this.alternatives = alternatives; }

    public void split(SplittableToken... alternativePairs) {
        for (SplittableToken token : alternativePairs) {
            alternatives.add(token);
        }
    }

    public boolean hasAlternatives() {
        return alternatives != null && !alternatives.isEmpty();
    }

    @Override
    public String toString() {
        return "SplittableToken{text='" + text + "', type=" + type + "}";
    }
}
