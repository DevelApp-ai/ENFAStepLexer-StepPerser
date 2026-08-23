/**
 * GrammarRule class for StepParser Java bindings
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

    public GrammarRule(String name, String definition, boolean isTerminal) {
        this.name = name;
        this.definition = definition;
        this.isTerminal = isTerminal;
        this.dependencies = new ArrayList<>();
    }

    public String getName() { return name; }
    public String getDefinition() { return definition; }
    public boolean isTerminal() { return isTerminal; }
    public List<String> getDependencies() { return dependencies; }
    public void addDependency(String dependency) { dependencies.add(dependency); }
}
