using System;
using System.Collections.Generic;
using DevelApp.StepLexer;
using CognitiveGraph;
using CognitiveGraph.Builder;
using CognitiveGraph.Accessors;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Context object passed to semantic action handlers during parse reductions
    /// Provides the sole API for actions to interact with the parsing state
    /// </summary>
    public class ActionContext
    {
        /// <summary>
        /// Gets the list of matched child nodes from the production rule
        /// Allows actions to access $1, $2, etc. from the grammar
        /// </summary>
        public List<GraphNodeRef> ChildNodes { get; init; } = new();

        /// <summary>
        /// Gets the current parse context with symbol table and context stack
        /// </summary>
        public ParseContext ParseContext { get; init; } = new();

        /// <summary>
        /// Gets the CognitiveGraphBuilder for manipulating the semantic graph
        /// </summary>
        public CognitiveGraphBuilder GraphBuilder { get; init; } = new();

        /// <summary>
        /// Gets the location of the matched rule in the source code
        /// </summary>
        public ICodeLocation Location { get; init; } = new CodeLocation();

        /// <summary>
        /// Gets the raw action text to be executed by the handler
        /// </summary>
        public string ActionText { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the production rule that triggered this action
        /// </summary>
        public string RuleName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets user-defined properties for passing data between actions
        /// </summary>
        public Dictionary<string, object> Properties { get; init; } = new();
    }

    /// <summary>
    /// Interface for pluggable semantic action handlers
    /// Allows custom execution strategies for grammar actions
    /// </summary>
    public interface ISemanticActionHandler
    {
        /// <summary>
        /// Gets the name of this handler (e.g., "csharp_script", "python", "javascript")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execute the semantic action with the given context
        /// </summary>
        /// <param name="context">The action context containing all necessary state</param>
        void Execute(ActionContext context);

        /// <summary>
        /// Validate that the action text can be executed by this handler
        /// Returns an error message if invalid, or null if valid
        /// </summary>
        /// <param name="actionText">The action text to validate</param>
        /// <returns>Error message or null if valid</returns>
        string? Validate(string actionText);
    }

    /// <summary>
    /// Default semantic action handler that simply stores action text
    /// Can be replaced with more sophisticated handlers like Roslyn-based execution
    /// </summary>
    public class DefaultSemanticActionHandler : ISemanticActionHandler
    {
        /// <inheritdoc/>
        public string Name => "default";

        /// <inheritdoc/>
        public void Execute(ActionContext context)
        {
            // Default implementation: no-op
            // In a full implementation, this could use Roslyn Scripting API
            // to execute C# code from the action text
            
            // Store the action execution in context for debugging/tracing
            context.Properties["ActionExecuted"] = true;
            context.Properties["ActionText"] = context.ActionText;
        }

        /// <inheritdoc/>
        public string? Validate(string actionText)
        {
            // Default validation: accept any text
            return null;
        }
    }

    /// <summary>
    /// Roslyn-based C# semantic action handler (placeholder for future Roslyn integration)
    /// </summary>
    public class RoslynSemanticActionHandler : ISemanticActionHandler
    {
        /// <inheritdoc/>
        public string Name => "csharp_script";

        /// <inheritdoc/>
        public void Execute(ActionContext context)
        {
            // TODO: Implement Roslyn Scripting API execution
            // This would compile and execute the C# code in context.ActionText
            // with access to context.ChildNodes, context.ParseContext, etc.
            
            // For now, just mark as executed
            context.Properties["ActionExecuted"] = true;
            context.Properties["ActionHandler"] = Name;
            context.Properties["ActionText"] = context.ActionText;
        }

        /// <inheritdoc/>
        public string? Validate(string actionText)
        {
            // TODO: Use Roslyn to parse and validate the C# syntax
            // For now, just check it's not empty
            if (string.IsNullOrWhiteSpace(actionText))
                return "Action text cannot be empty";
            
            return null;
        }
    }
}
