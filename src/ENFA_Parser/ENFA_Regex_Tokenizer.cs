using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    public class ENFA_Regex_Tokenizer : ENFA_Tokenizer
    {
        private List<char> _matchedCharInRegexBuild;
        private string _currectTerminalName;

        public ENFA_Regex_Tokenizer(ENFA_Controller controller):base(controller)
        { }

        public override bool Tokenize(string terminalName, StreamReader reader)
        {
            _matchedCharInRegexBuild = new List<char>();
            _currectTerminalName = terminalName;
            char? nextChar = NextCharInStream(reader);
            bool escaped = false;
            bool success = false; // Error until proven correct
            bool exit = false;
            ENFA_GroupingStart _parentStart = Controller.PatternStart;
            ENFA_PatternEnd _patternEnd = Controller.Factory.CreatePatternEnd(_parentStart as ENFA_PatternStart, terminalName, StateType.Accepting);
            ENFA_GroupingEnd _parentEnd = _patternEnd;
            ENFA_Base lastState = _parentStart;
            ENFA_Base nextState;
            ENFA_Transition activeTransition = null;
            while (nextChar.HasValue && !exit)
            {
                char? tempNextChar = PeekNextChar(reader);
                MatchingType matchingType = MatchingType.NotSet;
                if (!escaped)
                {
                    switch (nextChar.Value)
                    {
                        case Constants.Backslash:
                            escaped = true;
                            break;
                        case Constants.ExitContext:
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.GroupingEnd, _parentEnd);
                            lastState.AddTransition(activeTransition);
                            exit = true;
                            if (_parentEnd is ENFA_PatternEnd)
                            {
                                success = true;
                            }
                            else
                            {
                                ThrowBuildException(ErrorText.ExitContextBeforePatternEnd);
                            }
                            break;
                        case Constants.StartOfLine:
                            nextState = lastState.NewState(RegexTransitionType.StartOfLine.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.StartOfLine, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.AllButNewLine:
                            nextState = lastState.NewState(RegexTransitionType.NegateNewLine.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateNewLine, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.EndOfLine:
                            if (lastState is ENFA_PatternStart)
                            {
                                ThrowBuildException(ErrorText.EndOfLineAsFirstCharInPattern);
                            }
                            nextState = lastState.NewState(RegexTransitionType.EndOfLine.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.EndOfLine, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.RightCurlyBracket:
                            ThrowBuildException(ErrorText.RightCurlyBracketWithoutMatchingLeftCurlyBracket);
                            break;
                        case Constants.LeftCurlyBracket:
                            if (lastState is ENFA_PatternStart)
                            {
                                ThrowBuildException(ErrorText.LeftCurlyBracketAsFirstCharInPattern);
                            }
                            int minRepetitions;
                            int maxRepetitions;
                            CheckQuantifiers(reader, out minRepetitions, out maxRepetitions, out matchingType);
                            if (lastState is ENFA_GroupEnd)
                            {
                                SetTransitionCount((lastState as ENFA_GroupEnd).GroupStart, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            else
                            {
                                SetTransitionCount(lastState, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            break;
                        case Constants.RightSquareBracket:
                            ThrowBuildException(ErrorText.RightSquareBracketWithoutMatchingLeftSquareBracket);
                            break;
                        case Constants.LeftSquareBracket:
                            if (tempNextChar.HasValue && tempNextChar.Value == Constants.CircumflexAccent)
                            {
                                nextState = lastState.NewState("Negate Character Group", StateType.Transition);
                                /* Remove CircumfelxAccent */
                                ConsumeNextChar(reader);
                                activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateLiteral, nextState);
                            }
                            else
                            {
                                nextState = lastState.NewState("Character Group", StateType.Transition);
                                activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            }
                            AddCharacterGroup((ENFA_Regex_Transition)activeTransition, reader);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.GroupingStart:
                            bool recording = Controller.DefaultGroupingRecording;
                            string groupName = null;
                            if (tempNextChar.HasValue && tempNextChar.Value == Constants.QuestionMark)
                            {
                                /* Consume QuetionMark */
                                ConsumeNextChar(reader);
                                char? tempNextChar2 = PeekNextChar(reader);
                                switch (tempNextChar2.Value)
                                {
                                    case Constants.Colon:
                                        /* Consume Colon */
                                        ConsumeNextChar(reader);
                                        /* non-recording group */
                                        recording = false;
                                        _parentStart = Controller.Factory.CreateGroupStart(_parentStart);
                                        _parentEnd = Controller.Factory.CreateGroupEnd(_parentStart as ENFA_GroupStart, recording, groupName, _parentEnd);
                                        break;
                                    case Constants.GreaterThanSign:
                                        /* Consume Greater Than Sign */
                                        ConsumeNextChar(reader);
                                        /* recording group */
                                        recording = true;
                                        _parentStart = Controller.Factory.CreateGroupStart(_parentStart);
                                        _parentEnd = Controller.Factory.CreateGroupEnd(_parentStart as ENFA_GroupStart, recording, groupName, _parentEnd);
                                        break;
                                    case Constants.EqualsSign:
                                        /* Consume EqualSign */
                                        ConsumeNextChar(reader);
                                        /* positive lookahead */
                                        _parentStart = Controller.Factory.CreateLookaheadStart(AssertionType.Positive, _parentStart);
                                        _parentEnd = Controller.Factory.CreateLookaheadEnd(_parentStart as ENFA_LookaheadStart, _parentEnd);
                                        break;
                                    case Constants.ExclamationMark:
                                        /* Consume ExclamationMark */
                                        ConsumeNextChar(reader);
                                        /* negative lookahead */
                                        _parentStart = Controller.Factory.CreateLookaheadStart(AssertionType.Negative, _parentStart);
                                        _parentEnd = Controller.Factory.CreateLookaheadEnd(_parentStart as ENFA_LookaheadStart, _parentEnd);
                                        break;
                                    case Constants.LessThanSign:
                                        /* Consume Less Than Sign */
                                        ConsumeNextChar(reader);
                                        char? tempNextChar3 = PeekNextChar(reader);
                                        if (tempNextChar3.Value == Constants.EqualsSign)
                                        {
                                            /* Consume Equals Sign */
                                            ConsumeNextChar(reader);
                                            /* positive lookbehind */
                                            _parentStart = Controller.Factory.CreateLookbehindStart(AssertionType.Positive, _parentStart);
                                            _parentEnd = Controller.Factory.CreateLookbehindEnd(_parentStart as ENFA_LookbehindStart, _parentEnd);
                                        }
                                        else if (tempNextChar3.Value == Constants.ExclamationMark)
                                        {
                                            /* Consume Exclamation Mark */
                                            ConsumeNextChar(reader);
                                            /* negative lookbehind */
                                            _parentStart = Controller.Factory.CreateLookbehindStart(AssertionType.Negative, _parentStart);
                                            _parentEnd = Controller.Factory.CreateLookbehindEnd(_parentStart as ENFA_LookbehindStart, _parentEnd);
                                        }
                                        else
                                        {
                                            /* named group */
                                            recording = true;
                                            groupName = GetGroupName(reader);
                                            _parentStart = Controller.Factory.CreateGroupStart(_parentStart);
                                            _parentEnd = Controller.Factory.CreateGroupEnd(_parentStart as ENFA_GroupStart, recording, groupName, _parentEnd);
                                        }
                                        break;
                                    default:
                                        ThrowBuildException(ErrorText.GroupingExpectedSpecifierAfterQuestionMark);
                                        break;
                                }
                            }
                            else
                            {
                                _parentStart = Controller.Factory.CreateGroupStart(_parentStart);
                                _parentEnd = Controller.Factory.CreateGroupEnd(_parentStart as ENFA_GroupStart, recording, groupName, _parentEnd);
                            }
                            if (_parentEnd is ENFA_LookbehindEnd)
                            {
                                nextState = _parentEnd;
                            }
                            else
                            {
                                nextState = _parentStart;
                            }
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.GroupingStart, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.GroupingEnd:
                            if(!(_parentStart is ENFA_GroupingStart))
                            {
                                ThrowBuildException(ErrorText.GroupingEndWithoutGroupingStart);
                            }
                            if (_parentEnd is ENFA_LookbehindEnd)
                            {
                                nextState = _parentStart;
                            }
                            else
                            {
                                nextState = _parentEnd;
                            }
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.GroupingEnd, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            _parentStart = _parentStart.Parent;
                            _parentEnd = _parentEnd.Parent;
                            break;
                        case Constants.VerticalLine:
                            if (_parentEnd is ENFA_LookbehindEnd)
                            {
                                nextState = _parentEnd;
                            }
                            else
                            {
                                nextState = _parentStart;
                            }
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.GroupingEnd, _parentEnd);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.PlusSign:
                            if (lastState is ENFA_PatternStart)
                            {
                                ThrowBuildException(ErrorText.PlusSignAsFirstCharInPattern);
                            }
                            if (tempNextChar == Constants.QuestionMark)
                            {
                                /* Consume Quention Mark */
                                ConsumeNextChar(reader);
                                /* Lazy matching overwriting default */
                                matchingType = MatchingType.LazyMatching;
                            }
                            else if (tempNextChar == Constants.GreaterThanSign)
                            {
                                /* Consume Greater Than Sign */
                                ConsumeNextChar(reader);
                                /* Greedy matching overwriting default */
                                matchingType = MatchingType.GreedyMatching;
                            }
                            else
                            {
                                /* Use default matching */
                                matchingType = Controller.DefaultMatchType;
                            }
                            minRepetitions = 1;
                            maxRepetitions = -1;
                            if (lastState is ENFA_GroupEnd)
                            {
                                SetTransitionCount((lastState as ENFA_GroupEnd).GroupStart, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            else
                            {
                                SetTransitionCount(lastState, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            break;
                        case Constants.Asterisk:
                            if (lastState is ENFA_PatternStart)
                            {
                                ThrowBuildException(ErrorText.AsterixAsFirstCharInPattern);
                            }

                            if (tempNextChar == Constants.QuestionMark)
                            {
                                /* Consume Quention Mark */
                                ConsumeNextChar(reader);
                                /* Lazy matching overwriting default */
                                matchingType = MatchingType.LazyMatching;
                            }
                            else if (tempNextChar == Constants.GreaterThanSign)
                            {
                                /* Consume Greater Than Sign */
                                ConsumeNextChar(reader);
                                /* Greedy matching overwriting default */
                                matchingType = MatchingType.GreedyMatching;
                            }
                            else
                            {
                                /* Use default matching */
                                matchingType = Controller.DefaultMatchType;
                            }
                            minRepetitions = 0;
                            maxRepetitions = -1;
                            if (lastState is ENFA_GroupEnd)
                            {
                                SetTransitionCount((lastState as ENFA_GroupEnd).GroupStart, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            else
                            {
                                SetTransitionCount(lastState, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            break;
                        case Constants.QuestionMark:
                            if (lastState is ENFA_PatternStart)
                            {
                                ThrowBuildException(ErrorText.QuestionMarkAsFirstCharInPattern);
                            }
                            if (tempNextChar == Constants.QuestionMark)
                            {
                                /* Consume Quention Mark */
                                ConsumeNextChar(reader);
                                /* Lazy matching overwriting default */
                                matchingType = MatchingType.LazyMatching;
                            }
                            else if (tempNextChar == Constants.GreaterThanSign)
                            {
                                /* Consume Greater Than Sign */
                                ConsumeNextChar(reader);
                                /* Greedy matching overwriting default */
                                matchingType = MatchingType.GreedyMatching;
                            }
                            else
                            {
                                /* Use default matching */
                                matchingType = Controller.DefaultMatchType;
                            }
                            minRepetitions = 0;
                            maxRepetitions = 1;
                            if (lastState is ENFA_GroupEnd)
                            {
                                SetTransitionCount((lastState as ENFA_GroupEnd).GroupStart, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            else
                            {
                                SetTransitionCount(lastState, activeTransition, minRepetitions, maxRepetitions, matchingType);
                            }
                            break;
                        default:
                            nextState = lastState.NewState(nextChar.Value, StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(nextChar.Value);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                    }
                }
                else
                {
                    /* Escaped characters */
                    switch (nextChar.Value)
                    {
                        case '0':
                            nextState = lastState.NewState("Null Char", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.NullChar);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'a':
                            nextState = lastState.NewState("Alert", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.Alert);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'e':
                            nextState = lastState.NewState("Escape", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.Escape);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'y':
                            nextState = lastState.NewState("Backspace", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.Backspace);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'f':
                            nextState = lastState.NewState("Form Feed", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.FormFeed);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'r':
                            nextState = lastState.NewState("Carriage Return", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.CarriageReturn);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 't':
                            nextState = lastState.NewState("Horizontal Tab", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.HorizontalTab);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'v':
                            nextState = lastState.NewState("Vertical Tab", StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(Constants.VerticalTab);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'n':
                            nextState = lastState.NewState(RegexTransitionType.NewLine.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NewLine, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'w':
                            nextState = lastState.NewState(RegexTransitionType.Word.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Word, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'W':
                            nextState = lastState.NewState(RegexTransitionType.NegateWord.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateWord, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'd':
                            nextState = lastState.NewState(RegexTransitionType.Digit.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Digit, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'D':
                            nextState = lastState.NewState(RegexTransitionType.NegateDigit.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateDigit, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 's':
                            nextState = lastState.NewState(RegexTransitionType.Whitespace.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Whitespace, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'S':
                            nextState = lastState.NewState(RegexTransitionType.NegateWhitespace.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateWhitespace, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'l':
                            nextState = lastState.NewState(RegexTransitionType.Letter.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Letter, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'L':
                            nextState = lastState.NewState(RegexTransitionType.NegateLetter.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateLetter, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'b':
                            nextState = lastState.NewState(RegexTransitionType.WordBoundary.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.WordBoundary, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'B':
                            nextState = lastState.NewState(RegexTransitionType.NegateWordBoundary.ToString(), StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.NegateWordBoundary, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            /* Back reference */
                            int groupNumber = int.Parse(nextChar.Value.ToString());
                            nextState = lastState.NewPlaceHolder(_patternEnd.LookupGroupNameFromNumber(groupNumber));
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.BackReference, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case 'k':
                            /* Named back reference like \k<Bartho> */
                            string groupName = null;
                            if (tempNextChar.HasValue && tempNextChar.Value == Constants.LessThanSign)
                            {
                                /* Consume LessThanSign */
                                ConsumeNextChar(reader);
                                groupName = GetGroupName(reader);
                                if(!_patternEnd.GroupNameExists(groupName))
                                {
                                    ThrowBuildException(ErrorText.SpecifiedGroupNameDoesNotExist);
                                }
                            }
                            else
                            {
                                ThrowBuildException(ErrorText.NamedBackreferenceMissingStartGroupName);
                            }
                            nextState = lastState.NewPlaceHolder(groupName);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.BackReference, nextState);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        case Constants.DoubleQuote:
                        case Constants.LeftCurlyBracket:
                        case Constants.LeftSquareBracket:
                        case Constants.LeftParanthesis:
                        case Constants.RightParanthesis:
                        case Constants.VerticalLine:
                        case Constants.Backslash:
                        case Constants.FullStop:
                        case Constants.DollarSign:
                        case Constants.QuestionMark:
                        case Constants.PlusSign:
                        case Constants.Asterisk:
                        case Constants.CircumflexAccent:
                            nextState = lastState.NewState(nextChar.Value, StateType.Transition);
                            activeTransition = lastState.NewRegexTransition(RegexTransitionType.Literal, nextState);
                            (activeTransition as ENFA_Regex_Transition).AddLiteral(nextChar.Value);
                            lastState.AddTransition(activeTransition);
                            lastState = nextState;
                            break;
                        default:
                            ThrowBuildException(ErrorText.CharacterEscapedWithoutBeingExpectedTo);
                            break;
                    }
                    escaped = false;
                }
                if (!exit)
                {
                    nextChar = NextCharInStream(reader);
                }
            }
            return success;
        }

        private void SetTransitionCount(ENFA_Base state, ENFA_Transition transition, int minRepetitions, int maxRepetitions, MatchingType matchingType)
        {
            state.MinRepetitions = minRepetitions;
            state.MaxRepetitions = maxRepetitions;
            transition.MatchingType = matchingType;
        }

        private void ThrowBuildException(string message)
        {
            throw new ENFA_RegexBuild_Exception(_currectTerminalName, _matchedCharInRegexBuild.ToArray().ToString(), message);
        }

        private void CheckQuantifiers(StreamReader reader, out int minRepetitions, out int maxRepetitions, out MatchingType matchingType)
        {
            char matchedChar;
            minRepetitions = 0;
            maxRepetitions = -1;
            /* Run digits until comma or curly barce end */
            string firstDigit = GetStringUntilChar(reader, new char[] { Constants.Comma, Constants.RightCurlyBracket }, out matchedChar);
            string secondDigit = null;
            if (matchedChar == Constants.RightCurlyBracket && firstDigit == String.Empty)
            {
                ThrowBuildException(ErrorText.EmptyCurlyBraces);
            }
            if (firstDigit != String.Empty && !int.TryParse(firstDigit, out minRepetitions))
            {
                ThrowBuildException(ErrorText.CouldNotParseMinRepetitions);
            }
            else if (matchedChar == Constants.RightCurlyBracket)
            {
                maxRepetitions = minRepetitions;
            }
            if (matchedChar == Constants.Comma)
            {
                /* if comma is found then check digits until curly barce end */
                secondDigit = GetStringUntilChar(reader, new char[] { Constants.RightCurlyBracket }, out matchedChar);
                if (secondDigit == String.Empty)
                {
                    maxRepetitions = -1;
                }
                else if (!int.TryParse(secondDigit, out maxRepetitions))
                {
                    ThrowBuildException(ErrorText.CouldNotParseMaxRepetitions);
                }
            }
            /* Check for additional matching type */
            if (PeekNextChar(reader) == Constants.QuestionMark)
            {
                /* Consume Quention Mark */
                ConsumeNextChar(reader);
                /* Lazy matching overwriting default */
                matchingType = MatchingType.LazyMatching;
            }
            else if (PeekNextChar(reader) == Constants.GreaterThanSign)
            {
                /* Consume Greater Than Sign */
                ConsumeNextChar(reader);
                /* Greedy matching overwriting default */
                matchingType = MatchingType.GreedyMatching;
            }
            else
            {
                /* Use default matching */
                matchingType = Controller.DefaultMatchType;
            }
        }

        private string GetGroupName(StreamReader reader)
        {
            char matchedChar;
            string groupName = GetStringUntilChar(reader, new char[] { Constants.GreaterThanSign }, out matchedChar);
            if(string.IsNullOrEmpty(groupName))
            {
                ThrowBuildException(ErrorText.NamedGroupCannotBeEmpty);
            }
            return groupName;
        }

        private string GetStringUntilChar(StreamReader reader, char[] matchingChars, out char matchedChar)
        {
            StringBuilder matchedString = new StringBuilder();
            bool exit = false;
            char? tempMatchedChar = null;
            while (!exit)
            {
                char? nextChar = NextCharInStream(reader);
                if (!nextChar.HasValue)
                {
                    ThrowBuildException(ErrorText.EndOfStreamBeforeCharFound);
                }
                else
                {
                    foreach (char matchChar in matchingChars)
                    {
                        if (nextChar.Value == matchChar)
                        {
                            tempMatchedChar = nextChar.Value;
                            exit = true;
                            break;
                        }
                    }
                    if(!exit)
                    {
                        matchedString.Append(nextChar.Value);
                    }
                }
            }
            if(tempMatchedChar.HasValue)
            {
                matchedChar = tempMatchedChar.Value;
            }
            else
            {
                ThrowBuildException(ErrorText.EndOfStreamBeforeCharFound);
                matchedChar = Constants.NullChar;// Never hit this
            }
            return matchedString.ToString();
        }

        private void AddCharacterGroup(ENFA_Regex_Transition activeTransition, StreamReader reader)
        {
            bool exit = false;
            if(PeekNextChar(reader) == Constants.RightSquareBracket)
            {
                ThrowBuildException(ErrorText.CharacterClassEmpty);
            }
            while (!exit)
            {
                char? nextChar = NextCharInStream(reader);
                char? tempNextChar = PeekNextChar(reader);
                if (!nextChar.HasValue)
                {
                    exit = true;
                }
                else
                {
                    if(nextChar.Value == Constants.RightSquareBracket)
                    {
                        exit = true;
                    }
                    else if(nextChar.Value == Constants.Backslash)
                    {
                        if (tempNextChar.Value == 'n')
                        {
                            ConsumeNextChar(reader);// Consume New Line
                            activeTransition.AddLiteral(Constants.NewLine);
                        }
                        else if (tempNextChar.Value == 'v')
                        {
                            ConsumeNextChar(reader);// Consume Vertical Tab
                            activeTransition.AddLiteral(Constants.VerticalTab);
                        }
                        else if (tempNextChar.Value == 't')
                        {
                            ConsumeNextChar(reader);// Consume Horizontal Tab
                            activeTransition.AddLiteral(Constants.HorizontalTab);
                        }
                        else if (tempNextChar.Value == Constants.Backslash)
                        {
                            ConsumeNextChar(reader);// Consume Backslash
                            activeTransition.AddLiteral(Constants.Backslash);
                        }
                        else if (tempNextChar.Value == 'f')
                        {
                            ConsumeNextChar(reader);// Consume Form Feed
                            activeTransition.AddLiteral(Constants.FormFeed);
                        }
                        else if (tempNextChar.Value == 'r')
                        {
                            ConsumeNextChar(reader);// Consume Carriage Return
                            activeTransition.AddLiteral(Constants.CarriageReturn);
                        }
                        else if (tempNextChar.Value == '0')
                        {
                            ConsumeNextChar(reader);// Consume Null Char
                            activeTransition.AddLiteral(Constants.NullChar);
                        }
                        else if (tempNextChar.Value == 'a')
                        {
                            ConsumeNextChar(reader);// Consume Alert
                            activeTransition.AddLiteral(Constants.Alert);
                        }
                        else if (tempNextChar.Value == 'e')
                        {
                            ConsumeNextChar(reader);// Consume Escape
                            activeTransition.AddLiteral(Constants.Escape);
                        }
                        else if (tempNextChar.Value == 'y')
                        {
                            ConsumeNextChar(reader);// Consume Backspace
                            activeTransition.AddLiteral(Constants.Backspace);
                        }
                        else
                        {
                            ThrowBuildException(ErrorText.CharacterClassEscapedWithoutBeingExpectedTo);
                        }
                    }
                    else
                    {
                        if(tempNextChar.HasValue && tempNextChar.Value == Constants.HyphenMinusSign)
                        {
                            /* Char range */
                            ConsumeNextChar(reader);// Consume Hyphen Minus Sign
                            char? endRange = NextCharInStream(reader);
                            if(!endRange.HasValue)
                            {
                                ThrowBuildException(ErrorText.CharacterRangeHasNoEndValue);
                            }
                            int lowCounter = (int)nextChar.Value;
                            int highCounter = (int) endRange.Value;
                            if(lowCounter > highCounter)
                            {
                                int temp = lowCounter;
                                lowCounter = highCounter;
                                highCounter = temp;
                            }
                            for(int counter = lowCounter; counter <= highCounter; counter += 1)
                            {
                                activeTransition.AddLiteral((char)counter);
                            }
                        }
                        else
                        {
                            activeTransition.AddLiteral(nextChar.Value);
                        }
                    }
                }
            }
        }

        private char? PeekNextChar(StreamReader reader)
        {
            return (char?)reader.Peek();
        }

        private void ConsumeNextChar(StreamReader reader)
        {
            NextCharInStream(reader);
        }

        private char? NextCharInStream(StreamReader reader)
        {
            if (reader.EndOfStream)
            {
                return null;
            }
            else
            {
                char next = (char)reader.Read();
                _matchedCharInRegexBuild.Add(next);
                return next;
            }
        }
    }
}