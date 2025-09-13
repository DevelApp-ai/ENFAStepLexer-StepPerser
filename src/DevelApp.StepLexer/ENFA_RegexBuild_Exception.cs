using System;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Exception thrown during regex building operations in ENFA processing
    /// </summary>
    public class ENFA_RegexBuild_Exception : ENFA_Exception
    {
        /// <summary>
        /// Initializes a new instance of the ENFA_RegexBuild_Exception class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ENFA_RegexBuild_Exception(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ENFA_RegexBuild_Exception class with terminal information and error message
        /// </summary>
        /// <param name="terminalName">The name of the terminal that caused the error</param>
        /// <param name="matchedSofar">The portion of input matched so far</param>
        /// <param name="message">The message that describes the error</param>
        public ENFA_RegexBuild_Exception(string terminalName, string matchedSofar, string message) : base(string.Format("Terminal {0} [{1}]: {2}", terminalName, matchedSofar, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ENFA_RegexBuild_Exception class with terminal information, error message, and inner exception
        /// </summary>
        /// <param name="terminalName">The name of the terminal that caused the error</param>
        /// <param name="matchedSofar">The portion of input matched so far</param>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ENFA_RegexBuild_Exception(string terminalName, string matchedSofar, string message, Exception innerException) : base(string.Format("Terminal {0} [{1}]: {2}",terminalName, matchedSofar, message), innerException)
        {
        }
    }
}