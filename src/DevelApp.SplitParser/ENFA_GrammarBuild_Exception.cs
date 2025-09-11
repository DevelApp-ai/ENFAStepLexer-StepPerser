using System;
using System.Runtime.Serialization;

namespace DevelApp.SplitParser
{
    [Serializable]
    public class ENFA_GrammarBuild_Exception : ENFA_Exception
    {
        public ENFA_GrammarBuild_Exception(string message) : base(message)
        {
        }

        public ENFA_GrammarBuild_Exception(string nonTerminalName, string matchedSofar, string message) : base(string.Format("Non-Terminal {0} [{1}]: {2}", nonTerminalName, matchedSofar, message))
        {
        }

        public ENFA_GrammarBuild_Exception(string nonTerminalName, string matchedSofar, string message, Exception innerException) : base(string.Format("Non-Terminal {0} [{1}]: {2}",nonTerminalName, matchedSofar, message), innerException)
        {
        }

        protected ENFA_GrammarBuild_Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}