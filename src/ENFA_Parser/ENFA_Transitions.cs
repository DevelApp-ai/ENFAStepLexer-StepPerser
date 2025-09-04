using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public class ENFA_Regex_Transition : ENFA_Transition
    {
        private RegexTransitionType _transitionType;
        private List<char> _literals;

        public ENFA_Regex_Transition(RegexTransitionType transitionType, ENFA_Base nextState) : base(nextState)
        {
            _transitionType = transitionType;
            _literals = new List<char>();
        }

        public RegexTransitionType TransitionType
        {
            get
            {
                return _transitionType;
            }
        }

        public List<char> Literals
        {
            get
            {
                return _literals;
            }
        }

        public void AddLiteral(char literal)
        {
            _literals.Add(literal);
        }

        public override string ToString()
        {
            string result = _transitionType.ToString();
            if (_literals.Count > 0)
            {
                result += "[" + string.Join(",", _literals) + "]";
            }
            return result;
        }
    }

    public class ENFA_Grammar_Transition : ENFA_Transition
    {
        private GrammarTransitionType _transitionType;

        public ENFA_Grammar_Transition(GrammarTransitionType transitionType, ENFA_Base nextState) : base(nextState)
        {
            _transitionType = transitionType;
        }

        public GrammarTransitionType TransitionType
        {
            get
            {
                return _transitionType;
            }
        }

        public override string ToString()
        {
            return _transitionType.ToString();
        }
    }
}