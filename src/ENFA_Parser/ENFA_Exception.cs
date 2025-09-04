using System;
using System.IO;
using System.Runtime.Serialization;

namespace ENFA_Parser
{
    [Serializable]
    public class ENFA_Exception : Exception
    {
        public ENFA_Exception(string message
            , [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = ""
            , [System.Runtime.CompilerServices.CallerFilePath] string callerSourceFilePath = ""
            , [System.Runtime.CompilerServices.CallerLineNumber] int callerSourceLineNumber = 0
            ) : base(string.Format("{3}: {0} ({1}: {2})", callerMemberName, Path.GetFileName(callerSourceFilePath), callerSourceLineNumber, message))
        {
        }

        public ENFA_Exception(string message, Exception innerException
            , [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = ""
            , [System.Runtime.CompilerServices.CallerFilePath] string callerSourceFilePath = ""
            , [System.Runtime.CompilerServices.CallerLineNumber] int callerSourceLineNumber = 0
            ) : base(string.Format("{3}: {0} ({1}: {2})", callerMemberName, Path.GetFileName(callerSourceFilePath), callerSourceLineNumber, message), innerException)
        {
        }

        protected ENFA_Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}