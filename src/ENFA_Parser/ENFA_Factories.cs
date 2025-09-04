using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public class ENFA_Regex_Factory : ENFA_Factory
    {
        public ENFA_Regex_Factory(ENFA_Controller controller) : base(controller)
        {
        }

        public override ENFA_Tokenizer GetTokenizer()
        {
            return new ENFA_Regex_Tokenizer(Controller);
        }

        public override ENFA_Parser GetParser()
        {
            return new ENFA_Regex_Parser(Controller);
        }

        public override ENFA_Base CreateState(ENFA_Base previousState, string stateName, StateType stateType)
        {
            return new ENFA_State(Controller, previousState, stateName, stateType);
        }

        public override ENFA_PatternEnd CreatePatternEnd(ENFA_PatternStart patternStart, string terminalName, StateType stateType)
        {
            return new ENFA_PatternEnd(Controller, terminalName, stateType, null);
        }

        public override ENFA_GroupingStart CreateGroupStart(ENFA_GroupingStart parentStart)
        {
            return new ENFA_GroupStart(Controller, parentStart);
        }

        public override ENFA_GroupingEnd CreateGroupEnd(ENFA_GroupStart groupStart, bool recording, string groupName, ENFA_GroupingEnd parentEnd)
        {
            return new ENFA_GroupEnd(Controller, groupStart, parentEnd);
        }

        public override ENFA_GroupingStart CreateLookaheadStart(AssertionType assertionType, ENFA_GroupingStart parentStart)
        {
            return new ENFA_LookaheadStart(Controller, parentStart);
        }

        public override ENFA_GroupingEnd CreateLookaheadEnd(ENFA_LookaheadStart lookaheadStart, ENFA_GroupingEnd parentEnd)
        {
            return new ENFA_LookaheadEnd(Controller, parentEnd);
        }

        public override ENFA_GroupingStart CreateLookbehindStart(AssertionType assertionType, ENFA_GroupingStart parentStart)
        {
            return new ENFA_LookbehindStart(Controller, parentStart);
        }

        public override ENFA_GroupingEnd CreateLookbehindEnd(ENFA_LookbehindStart lookbehindStart, ENFA_GroupingEnd parentEnd)
        {
            return new ENFA_LookbehindEnd(Controller, parentEnd);
        }

        public override ENFA_Base CreatePlaceHolder(string groupName)
        {
            return new ENFA_PlaceHolder(Controller, groupName);
        }

        public ENFA_Regex_Transition CreateRegexTransition(RegexTransitionType transitionType, ENFA_Base nextState)
        {
            return new ENFA_Regex_Transition(transitionType, nextState);
        }
    }

    public class ENFA_Grammar_Factory : ENFA_Factory
    {
        public ENFA_Grammar_Factory(ENFA_Controller controller) : base(controller)
        {
        }

        public override ENFA_Tokenizer GetTokenizer()
        {
            return new ENFA_Grammar_Tokenizer(Controller);
        }

        public override ENFA_Parser GetParser()
        {
            return new ENFA_Grammar_Parser(Controller);
        }

        public override ENFA_Base CreateState(ENFA_Base previousState, string stateName, StateType stateType)
        {
            return new ENFA_State(Controller, previousState, stateName, stateType);
        }

        public override ENFA_PatternEnd CreatePatternEnd(ENFA_PatternStart patternStart, string terminalName, StateType stateType)
        {
            return new ENFA_PatternEnd(Controller, terminalName, stateType, null);
        }

        public override ENFA_GroupingStart CreateGroupStart(ENFA_GroupingStart parentStart)
        {
            return new ENFA_GroupStart(Controller, parentStart);
        }

        public override ENFA_GroupingEnd CreateGroupEnd(ENFA_GroupStart groupStart, bool recording, string groupName, ENFA_GroupingEnd parentEnd)
        {
            return new ENFA_GroupEnd(Controller, groupStart, parentEnd);
        }

        public override ENFA_GroupingStart CreateLookaheadStart(AssertionType assertionType, ENFA_GroupingStart parentStart)
        {
            return new ENFA_LookaheadStart(Controller, parentStart);
        }

        public override ENFA_GroupingEnd CreateLookaheadEnd(ENFA_LookaheadStart lookaheadStart, ENFA_GroupingEnd parentEnd)
        {
            return new ENFA_LookaheadEnd(Controller, parentEnd);
        }

        public override ENFA_GroupingStart CreateLookbehindStart(AssertionType assertionType, ENFA_GroupingStart parentStart)
        {
            return new ENFA_LookbehindStart(Controller, parentStart);
        }

        public override ENFA_GroupingEnd CreateLookbehindEnd(ENFA_LookbehindStart lookbehindStart, ENFA_GroupingEnd parentEnd)
        {
            return new ENFA_LookbehindEnd(Controller, parentEnd);
        }

        public override ENFA_Base CreatePlaceHolder(string groupName)
        {
            return new ENFA_PlaceHolder(Controller, groupName);
        }

        public ENFA_Grammar_Transition CreateGrammarTransition(GrammarTransitionType transitionType, ENFA_Base nextState)
        {
            return new ENFA_Grammar_Transition(transitionType, nextState);
        }
    }

    public class ENFA_Regex_Parser : ENFA_Parser
    {
        public ENFA_Regex_Parser(ENFA_Controller controller) : base(controller)
        {
        }
    }

    public class ENFA_Grammar_Parser : ENFA_Parser
    {
        public ENFA_Grammar_Parser(ENFA_Controller controller) : base(controller)
        {
        }
    }

    public class ENFA_Grammar_Tokenizer : ENFA_Tokenizer
    {
        public ENFA_Grammar_Tokenizer(ENFA_Controller controller) : base(controller)
        {
        }

        public override bool Tokenize(string terminalName, System.IO.StreamReader reader)
        {
            // Placeholder implementation
            return false;
        }
    }
}