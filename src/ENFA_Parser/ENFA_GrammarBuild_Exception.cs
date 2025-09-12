using System;

namespace ENFA_Parser
{
    /// <summary>
    /// Represents errors that occur during ENFA grammar building operations.
    /// Provides specialized exception handling for grammar construction and non-terminal processing.
    /// </summary>
    [Serializable]
    public class ENFA_GrammarBuild_Exception : ENFA_Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_GrammarBuild_Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ENFA_GrammarBuild_Exception(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_GrammarBuild_Exception"/> class with non-terminal name,
        /// matched content, and error message.
        /// </summary>
        /// <param name="nonTerminalName">The name of the non-terminal where the error occurred.</param>
        /// <param name="matchedSofar">The content matched before the error occurred.</param>
        /// <param name="message">The message that describes the error.</param>
        public ENFA_GrammarBuild_Exception(string nonTerminalName, string matchedSofar, string message) : base(string.Format("Non-Terminal {0} [{1}]: {2}", nonTerminalName, matchedSofar, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_GrammarBuild_Exception"/> class with non-terminal name,
        /// matched content, error message, and inner exception.
        /// </summary>
        /// <param name="nonTerminalName">The name of the non-terminal where the error occurred.</param>
        /// <param name="matchedSofar">The content matched before the error occurred.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ENFA_GrammarBuild_Exception(string nonTerminalName, string matchedSofar, string message, Exception innerException) : base(string.Format("Non-Terminal {0} [{1}]: {2}",nonTerminalName, matchedSofar, message), innerException)
        {
        }
    }
}