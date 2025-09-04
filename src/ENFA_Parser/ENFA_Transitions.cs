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
        private string? _unicodeProperty;
        private string? _posixCharClass;
        private int _unicodeCodePoint;
        private char _controlCharacter;

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

        public string? UnicodeProperty
        {
            get { return _unicodeProperty; }
            set { _unicodeProperty = value; }
        }

        public string? PosixCharClass  
        {
            get { return _posixCharClass; }
            set { _posixCharClass = value; }
        }

        public int UnicodeCodePoint
        {
            get { return _unicodeCodePoint; }
            set { _unicodeCodePoint = value; }
        }

        public char ControlCharacter
        {
            get { return _controlCharacter; }
            set { _controlCharacter = value; }
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
            if (!string.IsNullOrEmpty(_unicodeProperty))
            {
                result += "{" + _unicodeProperty + "}";
            }
            if (!string.IsNullOrEmpty(_posixCharClass))
            {
                result += "[:" + _posixCharClass + ":]";
            }
            if (_unicodeCodePoint > 0)
            {
                result += "\\x{" + _unicodeCodePoint.ToString("X") + "}";
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