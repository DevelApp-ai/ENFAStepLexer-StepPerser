using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public abstract class ENFA_Transition
    {
        //        A state transition can contain
        //- a counter max or min(* has min 0 and max -1, + has min 1 and max -1, ? has min 0 and max 1, { x,y}
        //        has min x and max y with x omitted has min 0 with y omitted has max -1, {x
        //    }
        //    has max = x and min = x)
        //- a scope identifier#
        //- a non-terminal#
        //- a terminal# matched on regex level
        // contextTransition
        private ENFA_Base _nextState;
        private MatchingType _matchingType;

        public ENFA_Transition(ENFA_Base nextState)
        {
            _nextState = nextState;
            _matchingType = MatchingType.NotSet;
        }

        public ENFA_Base NextState
        {
            get
            {
                return _nextState;
            }
        }

        public MatchingType MatchingType
        {
            get
            {
                return _matchingType;
            }
            set
            {
                _matchingType = value;
            }
        }
        public ENFA_Base Transition()
        {
            return _nextState;
        }

        internal void PrintHierarchy(StringBuilder stringBuilder, string indent, bool isLast)
        {
           _nextState.PrintHierarchy(stringBuilder, indent, isLast, ToString());
        }
    }
}