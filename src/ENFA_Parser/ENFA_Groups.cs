using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public abstract class ENFA_GroupingStart : ENFA_Base
    {
        private ENFA_GroupingStart _parent;

        public ENFA_GroupingStart(ENFA_Controller controller, StateType stateType, ENFA_GroupingStart parent) : base(controller, stateType)
        {
            _parent = parent;
        }

        public ENFA_GroupingStart Parent
        {
            get
            {
                return _parent;
            }
        }
    }

    public class ENFA_PatternStart : ENFA_GroupingStart
    {
        public ENFA_PatternStart(ENFA_Controller controller) : base(controller, StateType.NotApplicable, null)
        {
        }
    }

    public abstract class ENFA_GroupingEnd : ENFA_Base
    {
        private ENFA_GroupingEnd _parent;

        public ENFA_GroupingEnd(ENFA_Controller controller, StateType stateType, ENFA_GroupingEnd parent) : base(controller, stateType)
        {
            _parent = parent;
        }

        public ENFA_GroupingEnd Parent
        {
            get
            {
                return _parent;
            }
        }
    }

    public class ENFA_PatternEnd : ENFA_GroupingEnd
    {
        private string _terminalName;
        private List<string> _groupNames;

        public ENFA_PatternEnd(ENFA_Controller controller, string terminalName, StateType stateType, ENFA_GroupingEnd parent) : base(controller, stateType, parent)
        {
            _terminalName = terminalName;
            _groupNames = new List<string>();
        }

        public string LookupGroupNameFromNumber(int groupNumber)
        {
            if (groupNumber < 1 || groupNumber > _groupNames.Count)
            {
                throw new ENFA_Exception(ErrorText.LookupGroupNameFromNumberTooHighNumber);
            }
            return _groupNames[groupNumber - 1];
        }

        public bool GroupNameExists(string groupName)
        {
            return _groupNames.Contains(groupName);
        }
    }

    public class ENFA_GroupStart : ENFA_GroupingStart
    {
        public ENFA_GroupStart(ENFA_Controller controller, ENFA_GroupingStart parent) : base(controller, StateType.NotApplicable, parent)
        {
        }
    }

    public class ENFA_GroupEnd : ENFA_GroupingEnd
    {
        private ENFA_GroupStart _groupStart;

        public ENFA_GroupEnd(ENFA_Controller controller, ENFA_GroupStart groupStart, ENFA_GroupingEnd parent) : base(controller, StateType.NotApplicable, parent)
        {
            _groupStart = groupStart;
        }

        public ENFA_GroupStart GroupStart
        {
            get
            {
                return _groupStart;
            }
        }
    }

    public class ENFA_LookaheadStart : ENFA_GroupingStart
    {
        public ENFA_LookaheadStart(ENFA_Controller controller, ENFA_GroupingStart parent) : base(controller, StateType.NotApplicable, parent)
        {
        }
    }

    public class ENFA_LookaheadEnd : ENFA_GroupingEnd
    {
        public ENFA_LookaheadEnd(ENFA_Controller controller, ENFA_GroupingEnd parent) : base(controller, StateType.NotApplicable, parent)
        {
        }
    }

    public class ENFA_LookbehindStart : ENFA_GroupingStart
    {
        public ENFA_LookbehindStart(ENFA_Controller controller, ENFA_GroupingStart parent) : base(controller, StateType.NotApplicable, parent)
        {
        }
    }

    public class ENFA_LookbehindEnd : ENFA_GroupingEnd
    {
        public ENFA_LookbehindEnd(ENFA_Controller controller, ENFA_GroupingEnd parent) : base(controller, StateType.NotApplicable, parent)
        {
        }
    }

    public class ENFA_PlaceHolder : ENFA_Base
    {
        private string _groupName;

        public ENFA_PlaceHolder(ENFA_Controller controller, string groupName) : base(controller, StateType.NotApplicable)
        {
            _groupName = groupName;
        }

        public string GroupName
        {
            get
            {
                return _groupName;
            }
        }
    }

    public class ENFA_Match
    {
        // Placeholder for match results
    }
}