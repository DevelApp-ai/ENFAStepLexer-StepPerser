using System;
using System.IO;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Exception thrown by ENFA (Extended Non-deterministic Finite Automaton) operations
    /// </summary>
    public class ENFA_Exception : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ENFA_Exception class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="callerMemberName">The method or property name of the caller</param>
        /// <param name="callerSourceFilePath">The full path of the source file that contains the caller</param>
        /// <param name="callerSourceLineNumber">The line number in the source file at which the method is called</param>
        public ENFA_Exception(string message
            , [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = ""
            , [System.Runtime.CompilerServices.CallerFilePath] string callerSourceFilePath = ""
            , [System.Runtime.CompilerServices.CallerLineNumber] int callerSourceLineNumber = 0
            ) : base(string.Format("{3}: {0} ({1}: {2})", callerMemberName, Path.GetFileName(callerSourceFilePath), callerSourceLineNumber, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ENFA_Exception class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        /// <param name="callerMemberName">The method or property name of the caller</param>
        /// <param name="callerSourceFilePath">The full path of the source file that contains the caller</param>
        /// <param name="callerSourceLineNumber">The line number in the source file at which the method is called</param>
        public ENFA_Exception(string message, Exception innerException
            , [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = ""
            , [System.Runtime.CompilerServices.CallerFilePath] string callerSourceFilePath = ""
            , [System.Runtime.CompilerServices.CallerLineNumber] int callerSourceLineNumber = 0
            ) : base(string.Format("{3}: {0} ({1}: {2})", callerMemberName, Path.GetFileName(callerSourceFilePath), callerSourceLineNumber, message), innerException)
        {
        }
    }
}