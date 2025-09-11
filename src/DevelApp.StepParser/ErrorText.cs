using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelApp.StepParser
{
    public class ErrorText
    {
        public const string GroupingEndWithoutGroupingStart = "Grouping End without Grouping Start";
        public const string GroupingStartLookbehindMissingPositiveOrNegative = "Grouping Start Lookbehind missing Positive or Negative";
        public const string ExitContextBeforePatternEnd = "Exit Context before Pattern End";
        public const string NamedBackreferenceMissingStartGroupName = "Named Backreference missing Start Group Name (<)";
        public const string PlusSignAsFirstCharInPattern = "Plus sign as first char in pattern";
        public const string EndOfLineAsFirstCharInPattern = "End Of Line as first char in pattern";
        public const string RightCurlyBracketWithoutMatchingLeftCurlyBracket = "Right Curly Bracket without matching Left Curly Bracket";
        public const string LeftCurlyBracketAsFirstCharInPattern = "Left Curly Bracket as first char in pattern";
        public const string RightSquareBracketWithoutMatchingLeftSquareBracket = "Right Square Bracket without matching Left Square Bracket";
        public const string AsterixAsFirstCharInPattern = "Asterix as first char in pattern";
        public const string QuestionMarkAsFirstCharInPattern = "Question Mark as first char in pattern";
        public const string CharacterRangeHasNoEndValue = "Character Range has no end value";
        public const string EndOfStreamBeforeCharFound = "End Of Stream before pattern finished";
        public const string EmptyCurlyBraces = "Empty Curly Braces";
        public const string CouldNotParseMinRepetitions = "Could not parse min repetitions";
        public const string CouldNotParseMaxRepetitions = "Could not parse max repetitions";
        public const string CharacterEscapedWithoutBeingExpectedTo = "Character escaped without being expected to";
        public const string CharacterClassEscapedWithoutBeingExpectedTo = "Character Class escaped without being expected to";
        public const string CharacterClassEmpty = "Character Class empty";
        public const string GroupingExpectedSpecifierAfterQuestionMark = "Grouping expected specifier after Question Mark";
        public const string NamedGroupCannotBeEmpty = "Named Group cannot be empty";
        public const string LookupGroupNameFromNumberTooHighNumber = "Lookup Group Name from number too high number";
        public const string SpecifiedGroupNameDoesNotExist = "Specified Group Name does not exist";
        public const string TryingToCreateNewGrammarTransitionInRegex = "Trying to create new Grammar Transition in regex";
        public const string TryingToCreateNewRegexTransitionInGrammar = "Trying to create new Regex Transition in grammar";
        public const string PreviousStateIsNull = "Previous state is null";
        public const string ControllerGrammarTypeInRegex = "Controller GrammarType in regex";
    }
}