using System;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Exception thrown during grammar building operations in ENFA processing
    /// </summary>
    public class ENFA_GrammarBuild_Exception : ENFA_Exception
    {
        /// <summary>
        /// Initializes a new instance of the ENFA_GrammarBuild_Exception class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ENFA_GrammarBuild_Exception(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ENFA_GrammarBuild_Exception class with non-terminal information and error message
        /// </summary>
        /// <param name="nonTerminalName">The name of the non-terminal that caused the error</param>
        /// <param name="matchedSofar">The portion of input matched so far</param>
        /// <param name="message">The message that describes the error</param>
        public ENFA_GrammarBuild_Exception(string nonTerminalName, string matchedSofar, string message) : base(string.Format("Non-Terminal {0} [{1}]: {2}", nonTerminalName, matchedSofar, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ENFA_GrammarBuild_Exception class with non-terminal information, error message, and inner exception
        /// </summary>
        /// <param name="nonTerminalName">The name of the non-terminal that caused the error</param>
        /// <param name="matchedSofar">The portion of input matched so far</param>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ENFA_GrammarBuild_Exception(string nonTerminalName, string matchedSofar, string message, Exception innerException) : base(string.Format("Non-Terminal {0} [{1}]: {2}",nonTerminalName, matchedSofar, message), innerException)
        {
        }
    }
}