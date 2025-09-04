using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public class ENFA_Controller
    {
        private ParserType _parserType;
        private ENFA_Tokenizer _tokenizer;
        private ENFA_Parser _parser;
        private MatchingType _matchingType;
        private ENFA_PatternStart _patternStart;
        private bool _defaultGroupingRecording;
        private bool _inDebugMode;
        private ENFA_Factory _factory;

        public ENFA_Controller(ParserType parserType)
        {
            _parserType = parserType;
            switch(_parserType)
            {
                case ParserType.Regex:
                    _factory = new ENFA_Regex_Factory(this);
                    _tokenizer = Factory.GetTokenizer();
                    _parser = Factory.GetParser();
                    break;
                case ParserType.Grammar:
                    _factory = new ENFA_Grammar_Factory(this);
                    _tokenizer = Factory.GetTokenizer();
                    _parser = Factory.GetParser();
                    break;
            }
            _patternStart = new ENFA_PatternStart(this);
            _matchingType = MatchingType.LazyMatching;
            _inDebugMode = false;
        }

        public MatchingType DefaultMatchType
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

        public ParserType ParserType
        {
            get
            {
                return _parserType;
            }
        }

        public ENFA_Factory Factory
        {
            get
            {
                return _factory;
            }
        }

        public ENFA_Parser Parser
        {
            get
            {
                return _parser;
            }
        }

        public ENFA_PatternStart PatternStart
        {
            get
            {
                return _patternStart;
            }
        }

        public bool DefaultGroupingRecording
        {
            get
            {
                return _defaultGroupingRecording;
            }
            set
            {
                _defaultGroupingRecording = value;
            }
        }

        public bool InDebugMode
        {
            get
            {
                return _inDebugMode;
            }
            set
            {
                _inDebugMode = value;
            }
        }

        public ENFA_Tokenizer Tokenizer
        {
            get
            {
                return _tokenizer;
            }
            set
            {
                _tokenizer = value;
            }
        }

        public string PrintHierarchy
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                PatternStart.PrintHierarchy(stringBuilder, string.Empty,true, string.Empty);
                return stringBuilder.ToString();
            }
        }
        //private StreamReader _streamReader;

        //public void AddStream(Stream stream)
        //{
        //    _streamReader = new StreamReader(new BufferedStream(stream, Constants.BufferSize), Constants.StreamEncoding, Constants.DetectStreamEncodingFromByteOrderMarks);
        //}

        //public StreamReader Reader
        //{
        //    get
        //    {
        //        return _streamReader;
        //    }
        //}

        //public IEnumerable<ENFA_Base> Match(Stream stream)
        //{
        //    yield return new ENFA_ResetCount(StateType.Negating);
        //}
    }
}