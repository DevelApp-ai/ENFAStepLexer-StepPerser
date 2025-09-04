using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public abstract class ENFA_Tokenizer
    {
        private ENFA_Controller _controller;

        public ENFA_Tokenizer(ENFA_Controller controller)
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

        public abstract bool Tokenize(string terminalName, StreamReader reader);
    }
}