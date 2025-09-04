using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENFA_Parser
{
    public class ENFA_State:ENFA_Base
    {
        private string _stateName;
        private ENFA_Base _prevoiusState;

        public ENFA_State(ENFA_Controller controller, ENFA_Base prevoiusState, string stateName, StateType stateType):base(controller,stateType)
        {
            _stateName = stateName;
            _prevoiusState = prevoiusState;
        }

        public override string StateName
        {
            get
            {
                return _stateName;
            }
        }

        public ENFA_Base PrevoiusState
        {
            get
            {
                if(_prevoiusState == null)
                {
                    throw new ENFA_Exception(ErrorText.PreviousStateIsNull);
                }
                return _prevoiusState;
            }
        }
    }
}