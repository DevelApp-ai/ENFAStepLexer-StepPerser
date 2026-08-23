/**
 * GrammarRule class for StepParser Java bindings
 * Represents a grammar rule for parsing
 */
package ai.develapp.stepparser;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a grammar rule for the StepParser
 */
public class GrammarRule {
    private final String name;
    private final String definition;
    private final boolean isTerminal;
    private final List<String> dependencies;

    /**
     * Creates a new GrammarRule
     * @param name The name of the rule
     * @param definition The definition of the rule
     * @param isTerminal Whether this is a terminal rule
     */
    public GrammarRule(String name, String definition, boolean isTerminal) {
        this.name = name;
        this.definition = definition;
        this.isTerminal = isTerminal;
        this.dependencies = new ArrayList<>();
    }

    /**
     * Gets the name of the rule
     * @return The name
     */
    public String getName() {
        return name;
    }

    /**
     * Gets the definition of the rule
     * @return The definition
     */
    public String getDefinition() {
        return definition;
    }

    /**
     * Gets whether this is a terminal rule
     * @return true if terminal
     */
    public boolean isTerminal() {
        return isTerminal;
    }

    /**
     * Gets the dependencies of this rule
     * @return The list of dependencies
     */
    public List<String> getDependencies() {
        return dependencies;
    }

    /**
     * Adds a dependency to this rule
     * @param dependency The dependency to add
     */
    public void addDependency(String dependency) {
        dependencies.add(dependency);
    }

    @Override
    public String toString() {
        return "GrammarRule{" +
                "name='" + name + '\'' +
                ", definition='" + definition + '\'' +
                ", isTerminal=" + isTerminal +
                '}';
    }
}
