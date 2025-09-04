using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public abstract class ENFA_Parser
    {
        private ENFA_Controller _controller;

        public ENFA_Parser(ENFA_Controller controller)
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
    }
}