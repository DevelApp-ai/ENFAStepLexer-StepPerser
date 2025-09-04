using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public abstract class ENFA_Factory
    {
        private ENFA_Controller _controller;

        public ENFA_Factory(ENFA_Controller controller)
        {
            _controller = controller;
        }

        public ENFA_Controller Controller
        {
            get
            {
                return _controller;
            }
        }

        public abstract ENFA_Tokenizer GetTokenizer();

        public abstract ENFA_Base CreateState(ENFA_Base prevoiusState, string stateName, StateType stateType);

        public abstract ENFA_PatternEnd CreatePatternEnd(ENFA_PatternStart patternStart, string terminalName, StateType stateType);

        public abstract ENFA_GroupingStart CreateGroupStart(ENFA_GroupingStart parentStart);

        public abstract ENFA_Parser GetParser();

        public abstract ENFA_GroupingEnd CreateGroupEnd(ENFA_GroupStart groupStart, bool recording, string groupName, ENFA_GroupingEnd parentEnd);

        public abstract ENFA_GroupingStart CreateLookaheadStart(AssertionType positive, ENFA_GroupingStart parentStart);

        public abstract ENFA_GroupingEnd CreateLookaheadEnd(ENFA_LookaheadStart lookaheadStart, ENFA_GroupingEnd parentEnd);

        public abstract ENFA_GroupingStart CreateLookbehindStart(AssertionType positive, ENFA_GroupingStart parentStart);

        public abstract ENFA_GroupingEnd CreateLookbehindEnd(ENFA_LookbehindStart lookbehindStart, ENFA_GroupingEnd parentEnd);

        public abstract ENFA_Base CreatePlaceHolder(string groupName);

        public List<ENFA_Match> CreateMatchList()
        {
            return new List<ENFA_Match>();
        }
    }
}