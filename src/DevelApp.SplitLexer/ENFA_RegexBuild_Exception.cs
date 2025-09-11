using System;
using System.Runtime.Serialization;

namespace DevelApp.SplitLexer
{
    [Serializable]
    public class ENFA_RegexBuild_Exception : ENFA_Exception
    {
        public ENFA_RegexBuild_Exception(string message) : base(message)
        {
        }

        public ENFA_RegexBuild_Exception(string terminalName, string matchedSofar, string message) : base(string.Format("Terminal {0} [{1}]: {2}", terminalName, matchedSofar, message))
        {
        }

        public ENFA_RegexBuild_Exception(string terminalName, string matchedSofar, string message, Exception innerException) : base(string.Format("Terminal {0} [{1}]: {2}",terminalName, matchedSofar, message), innerException)
        {
        }

        protected ENFA_RegexBuild_Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}