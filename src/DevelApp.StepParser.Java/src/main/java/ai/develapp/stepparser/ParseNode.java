/**
 * ParseNode class for StepParser Java bindings
 * Represents a node in the parse tree
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

    /**
     * Creates a new ParseNode
     * @param type The type of the node
     * @param text The text content of the node
     * @param position The position of the node in the input
     * @param length The length of the node in the input
     */
    public ParseNode(String type, String text, int position, int length) {
        this.type = type;
        this.text = text;
        this.position = position;
        this.length = length;
        this.children = new ArrayList<>();
    }

    /**
     * Creates a new ParseNode with a token
     * @param token The token for this node
     */
    public ParseNode(SplittableToken token) {
        this(token.getType().toString(), token.getText(), token.getPosition(), token.getText().length());
        this.token = token;
    }

    /**
     * Gets the type of the node
     * @return The type
     */
    public String getType() {
        return type;
    }

    /**
     * Gets the text content of the node
     * @return The text
     */
    public String getText() {
        return text;
    }

    /**
     * Gets the position of the node in the input
     * @return The position
     */
    public int getPosition() {
        return position;
    }

    /**
     * Gets the length of the node in the input
     * @return The length
     */
    public int getLength() {
        return length;
    }

    /**
     * Gets the children of this node
     * @return The list of children
     */
    public List<ParseNode> getChildren() {
        return children;
    }

    /**
     * Adds a child to this node
     * @param child The child to add
     */
    public void addChild(ParseNode child) {
        children.add(child);
    }

    /**
     * Gets the token associated with this node
     * @return The token, or null if none
     */
    public SplittableToken getToken() {
        return token;
    }

    /**
     * Sets the token for this node
     * @param token The token
     */
    public void setToken(SplittableToken token) {
        this.token = token;
    }

    /**
     * Gets whether this node is a leaf node
     * @return true if this node has no children
     */
    public boolean isLeaf() {
        return children.isEmpty();
    }

    /**
     * Gets whether this node is a terminal node
     * @return true if this node is a terminal
     */
    public boolean isTerminal() {
        return token != null;
    }

    @Override
    public String toString() {
        return "ParseNode{" +
                "type='" + type + '\'' +
                ", text='" + text + '\'' +
                ", position=" + position +
                ", length=" + length +
                ", children=" + children.size() +
                '}';
    }
}
