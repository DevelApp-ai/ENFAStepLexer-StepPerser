using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public enum StateType
    {
        Negating,
        Accepting,
        Transition,
        Error,
        NotApplicable
    }

    public enum StateName
    {
        ENFA_PatternEnd
    }

    public enum ParserType
    {
        Regex,
        Grammar
    }

    public enum RegexTransitionType
    {
        Literal,
        NegateLiteral,
        Letter,
        NegateLetter,
        Digit,
        NegateDigit,
        Whitespace,
        NegateWhitespace,
        Word,
        NegateWord,
        NewLine,
        NegateNewLine,
        WordBoundary,
        NegateWordBoundary,
        StartOfLine,
        EndOfLine,
        ExitContext,
        BackReference,
        GroupingStart,
        GroupingEnd
    }

    public enum GrammarTransitionType
    {
        SwitchContext,
        BackReference,
        GroupingStart,
        GroupingEnd
    }

    public enum MatchingType
    {
        NotSet,
        LazyMatching,
        GreedyMatching
    }

    public enum AssertionType
    {
        Positive,
        Negative
    }
}