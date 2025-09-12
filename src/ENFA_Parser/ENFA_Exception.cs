using System;
using System.IO;

namespace ENFA_Parser
{
    /// <summary>
    /// Represents errors that occur during ENFA (Extended Non-deterministic Finite Automaton) parsing operations.
    /// Provides enhanced exception information with caller context details for debugging purposes.
    /// </summary>
    [Serializable]
    public class ENFA_Exception : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_Exception"/> class with a specified error message 
        /// and automatic caller context information.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="callerMemberName">The name of the member that called this constructor (automatically filled).</param>
        /// <param name="callerSourceFilePath">The full path of the source file that contains the caller (automatically filled).</param>
        /// <param name="callerSourceLineNumber">The line number in the source file at which this constructor is called (automatically filled).</param>
        public ENFA_Exception(string message
            , [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = ""
            , [System.Runtime.CompilerServices.CallerFilePath] string callerSourceFilePath = ""
            , [System.Runtime.CompilerServices.CallerLineNumber] int callerSourceLineNumber = 0
            ) : base(string.Format("{3}: {0} ({1}: {2})", callerMemberName, Path.GetFileName(callerSourceFilePath), callerSourceLineNumber, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ENFA_Exception"/> class with a specified error message,
        /// inner exception, and automatic caller context information.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="callerMemberName">The name of the member that called this constructor (automatically filled).</param>
        /// <param name="callerSourceFilePath">The full path of the source file that contains the caller (automatically filled).</param>
        /// <param name="callerSourceLineNumber">The line number in the source file at which this constructor is called (automatically filled).</param>
        public ENFA_Exception(string message, Exception innerException
            , [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = ""
            , [System.Runtime.CompilerServices.CallerFilePath] string callerSourceFilePath = ""
            , [System.Runtime.CompilerServices.CallerLineNumber] int callerSourceLineNumber = 0
            ) : base(string.Format("{3}: {0} ({1}: {2})", callerMemberName, Path.GetFileName(callerSourceFilePath), callerSourceLineNumber, message), innerException)
        {
        }
    }
}