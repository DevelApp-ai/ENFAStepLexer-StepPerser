/**
 * ParseNode class for StepParser Java bindings
 */
package ai.develapp.stepparser;

import ai.develapp.steplexer.SplittableToken;
import java.util.ArrayList;
import java.util.List;

/**
 * Represents a node in the parse tree
 */
public class ParseNode {
    private final String type;
    private final String text;
    private final int position;
    private final int length;
    private final List<ParseNode> children;
    private SplittableToken token;

    public ParseNode(String type, String text, int position, int length) {
        this.type = type;
        this.text = text;
        this.position = position;
        this.length = length;
        this.children = new ArrayList<>();
    }

    public ParseNode(SplittableToken token) {
        this(token.getType().toString(), token.getText(), token.getPosition(), token.getText().length());
        this.token = token;
    }

    public String getType() { return type; }
    public String getText() { return text; }
    public int getPosition() { return position; }
    public int getLength() { return length; }
    public List<ParseNode> getChildren() { return children; }
    public void addChild(ParseNode child) { children.add(child); }
    public SplittableToken getToken() { return token; }
    public void setToken(SplittableToken token) { this.token = token; }
    public boolean isLeaf() { return children.isEmpty(); }
}
