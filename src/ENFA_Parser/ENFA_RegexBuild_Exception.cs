using System;

namespace ENFA_Parser
{
    /// <summary>
    /// Represents errors that occur during ENFA regex pattern building operations.
    /// Provides specialized exception handling for regex construction and terminal matching.
    /// </summary>
    [Serializable]
    public class ENFA_RegexBuild_Exception : ENFA_Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_RegexBuild_Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ENFA_RegexBuild_Exception(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_RegexBuild_Exception"/> class with terminal name, 
        /// matched content, and error message.
        /// </summary>
        /// <param name="terminalName">The name of the terminal where the error occurred.</param>
        /// <param name="matchedSofar">The content matched before the error occurred.</param>
        /// <param name="message">The message that describes the error.</param>
        public ENFA_RegexBuild_Exception(string terminalName, string matchedSofar, string message) : base(string.Format("Terminal {0} [{1}]: {2}", terminalName, matchedSofar, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_RegexBuild_Exception"/> class with terminal name,
        /// matched content, error message, and inner exception.
        /// </summary>
        /// <param name="terminalName">The name of the terminal where the error occurred.</param>
        /// <param name="matchedSofar">The content matched before the error occurred.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ENFA_RegexBuild_Exception(string terminalName, string matchedSofar, string message, Exception innerException) : base(string.Format("Terminal {0} [{1}]: {2}",terminalName, matchedSofar, message), innerException)
        {
        }
    }
}