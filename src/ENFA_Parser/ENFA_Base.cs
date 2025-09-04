using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public abstract class ENFA_Base
    {
        //A state can be a negating state which terminates the state-visitor path
        //A state can be an accepting state which is a match
        //A state can be a transition state(not negating or accepting)

        private StateType _stateType;
        protected List<ENFA_Transition> _nextTransitions;
        private ENFA_Controller _controller;
        private int _minRepetitions;
        private int _maxRepetitions;

        public ENFA_Base(ENFA_Controller controller, StateType stateType)
        {
            _controller = controller;
            _stateType = stateType;
            _nextTransitions = new List<ENFA_Transition>();
            _minRepetitions = 1;
            _maxRepetitions = 1;
        }

        public StateType StateType
        {
            get
            {
                return _stateType;
            }
        }

        public virtual string StateName
        {
            get
            {
                return GetType().Name;
            }
        }

        public ENFA_Controller Controller
        {
            get
            {
                return _controller;
            }
        }

        public void AddTransition(ENFA_Transition nextTransition)
        {
            _nextTransitions.Add(nextTransition);
        }

        public List<ENFA_Transition> GetTransitions
        {
            get
            {
                return _nextTransitions;
            }
        }

        internal virtual ENFA_Transition NewGrammarTransition(GrammarTransitionType transitionType, ENFA_Base nextState)
        {
            if (Controller.ParserType == ParserType.Regex)
            {
                throw new ENFA_RegexBuild_Exception(ErrorText.TryingToCreateNewGrammarTransitionInRegex);
            }
            foreach (ENFA_Transition transition in GetTransitions)
            {
                ENFA_Base state = transition.Transition();
                if ((transition as ENFA_Grammar_Transition).TransitionType == transitionType && state.Equals(nextState))
                {
                    return transition;
                }
            }
            return (Controller.Factory as ENFA_Grammar_Factory).CreateGrammarTransition(transitionType, nextState);
        }

        internal virtual ENFA_Transition NewRegexTransition(RegexTransitionType transitionType, ENFA_Base nextState)
        {
            if(Controller.ParserType == ParserType.Grammar)
            {
                throw new ENFA_GrammarBuild_Exception(ErrorText.TryingToCreateNewRegexTransitionInGrammar);
            }
            foreach (ENFA_Transition transition in GetTransitions)
            {
                ENFA_Base state = transition.Transition();
                if ((transition as ENFA_Regex_Transition).TransitionType == transitionType && state.Equals(nextState))
                {
                    return transition;
                }
            }
            return (Controller.Factory as ENFA_Regex_Factory).CreateRegexTransition(transitionType, nextState);
        }

        internal virtual ENFA_Base NewState(char stateName, StateType stateType)
        {
            return NewState(stateName.ToString(), stateType);
        }

        internal virtual ENFA_Base NewState(string stateName, StateType stateType)
        {
            foreach(ENFA_Transition transition in GetTransitions)
            {
                ENFA_Base state = transition.Transition();
                if(state.StateName.Equals(StateName) && state.StateType == stateType)
                {
                    return state;
                }
            }
            return Controller.Factory.CreateState(this, stateName, stateType);
        }

        internal virtual ENFA_Base NewPlaceHolder(string groupName)
        {
            return Controller.Factory.CreatePlaceHolder(groupName);
        }

        public int MinRepetitions
        {
            get
            {
                return _minRepetitions;
            }
            set
            {
                _minRepetitions = value;
            }
        }

        public int MaxRepetitions
        {
            get
            {
                return _maxRepetitions;
            }
            set
            {
                _maxRepetitions = value;
            }
        }

        internal void PrintHierarchy(StringBuilder stringBuilder, string indent, bool isLast, string transitionName)
        {
            int maxCounter = GetTransitions.Count - 1;
            stringBuilder.AppendLine(string.Format("{0}{1}{2}{3}{4}"
                    , indent + (isLast ? Constants.BendPipe: Constants.TPipe)
                    , maxCounter == 0?Constants.HorizontalTPipe:Constants.HorizontalPipe
                    , transitionName
                    , Constants.HorizontalPipe
                    , ToString()));

            ENFA_Transition transition;
            indent += isLast ? Constants.Space : Constants.VerticalPipe;
            for(int counter = 0; counter <= maxCounter; counter += 1)
            {
                transition = GetTransitions[counter];
                if(counter == maxCounter)
                {
                    transition.PrintHierarchy(stringBuilder, indent,true);
                }
                else
                {
                    transition.PrintHierarchy(stringBuilder, indent, false);
                }
            }
        }

        public override string ToString()
        {
            string repetitions = String.Empty;
            if (MinRepetitions != 1 || MaxRepetitions != 1)
            {
                repetitions = "{" + MinRepetitions.ToString() + "," + MaxRepetitions.ToString() + "}";
            }
            return string.Format("{0}{1}{2}"
                    , StateName
                    , StateType == StateType.NotApplicable?string.Empty:"(" + StateType.ToString() + ")"
                    , repetitions);
        }
    }
}