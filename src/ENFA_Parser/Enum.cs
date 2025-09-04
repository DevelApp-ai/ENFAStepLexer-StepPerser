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
        StartOfString,      // \A
        EndOfString,        // \Z  
        AbsoluteEndOfString, // \z
        ContinueFromPreviousMatch, // \G
        AnyUnicodeNewline,  // \R
        ExitContext,
        BackReference,
        GroupingStart,
        GroupingEnd,
        UnicodeProperty,    // \p{...}
        NegateUnicodeProperty, // \P{...}
        PosixCharClass,     // [:alpha:], etc.
        NegatePosixCharClass,
        UnicodeCodePoint,   // \x{...}
        ControlCharacter    // \c[A-Z]
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