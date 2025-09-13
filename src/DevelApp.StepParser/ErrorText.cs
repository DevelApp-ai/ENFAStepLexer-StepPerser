using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Static error message constants for StepParser operations.
    /// Provides descriptive error messages for various parsing and grammar issues.
    /// </summary>
    public class ErrorText
    {
        /// <summary>Error message for grouping end without corresponding grouping start.</summary>
        public const string GroupingEndWithoutGroupingStart = "Grouping End without Grouping Start";
        
        /// <summary>Error message for grouping start lookbehind missing positive or negative indicator.</summary>
        public const string GroupingStartLookbehindMissingPositiveOrNegative = "Grouping Start Lookbehind missing Positive or Negative";
        
        /// <summary>Error message for exit context encountered before pattern end.</summary>
        public const string ExitContextBeforePatternEnd = "Exit Context before Pattern End";
        
        /// <summary>Error message for named backreference missing start group name (&lt;).</summary>
        public const string NamedBackreferenceMissingStartGroupName = "Named Backreference missing Start Group Name (<)";
        
        /// <summary>Error message for plus sign as first character in pattern.</summary>
        public const string PlusSignAsFirstCharInPattern = "Plus sign as first char in pattern";
        
        /// <summary>Error message for end of line as first character in pattern.</summary>
        public const string EndOfLineAsFirstCharInPattern = "End Of Line as first char in pattern";
        
        /// <summary>Error message for right curly bracket without matching left curly bracket.</summary>
        public const string RightCurlyBracketWithoutMatchingLeftCurlyBracket = "Right Curly Bracket without matching Left Curly Bracket";
        
        /// <summary>Error message for left curly bracket as first character in pattern.</summary>
        public const string LeftCurlyBracketAsFirstCharInPattern = "Left Curly Bracket as first char in pattern";
        
        /// <summary>Error message for right square bracket without matching left square bracket.</summary>
        public const string RightSquareBracketWithoutMatchingLeftSquareBracket = "Right Square Bracket without matching Left Square Bracket";
        
        /// <summary>Error message for asterisk as first character in pattern.</summary>
        public const string AsterixAsFirstCharInPattern = "Asterix as first char in pattern";
        
        /// <summary>Error message for question mark as first character in pattern.</summary>
        public const string QuestionMarkAsFirstCharInPattern = "Question Mark as first char in pattern";
        
        /// <summary>Error message for character range with no end value.</summary>
        public const string CharacterRangeHasNoEndValue = "Character Range has no end value";
        
        /// <summary>Error message for end of stream before pattern finished.</summary>
        public const string EndOfStreamBeforeCharFound = "End Of Stream before pattern finished";
        
        /// <summary>Error message for empty curly braces in pattern.</summary>
        public const string EmptyCurlyBraces = "Empty Curly Braces";
        
        /// <summary>Error message for unparseable minimum repetitions value.</summary>
        public const string CouldNotParseMinRepetitions = "Could not parse min repetitions";
        
        /// <summary>Error message for unparseable maximum repetitions value.</summary>
        public const string CouldNotParseMaxRepetitions = "Could not parse max repetitions";
        
        /// <summary>Error message for character escaped without being expected to.</summary>
        public const string CharacterEscapedWithoutBeingExpectedTo = "Character escaped without being expected to";
        
        /// <summary>Error message for character class escaped without being expected to.</summary>
        public const string CharacterClassEscapedWithoutBeingExpectedTo = "Character Class escaped without being expected to";
        
        /// <summary>Error message for empty character class definition.</summary>
        public const string CharacterClassEmpty = "Character Class empty";
        
        /// <summary>Error message for grouping expected specifier after question mark.</summary>
        public const string GroupingExpectedSpecifierAfterQuestionMark = "Grouping expected specifier after Question Mark";
        
        /// <summary>Error message for named group that cannot be empty.</summary>
        public const string NamedGroupCannotBeEmpty = "Named Group cannot be empty";
        
        /// <summary>Error message for group name lookup number too high.</summary>
        public const string LookupGroupNameFromNumberTooHighNumber = "Lookup Group Name from number too high number";
        
        /// <summary>Error message for specified group name that does not exist.</summary>
        public const string SpecifiedGroupNameDoesNotExist = "Specified Group Name does not exist";
        
        /// <summary>Error message for trying to create new grammar transition in regex context.</summary>
        public const string TryingToCreateNewGrammarTransitionInRegex = "Trying to create new Grammar Transition in regex";
        
        /// <summary>Error message for trying to create new regex transition in grammar context.</summary>
        public const string TryingToCreateNewRegexTransitionInGrammar = "Trying to create new Regex Transition in grammar";
        
        /// <summary>Error message for previous state being null when it shouldn't be.</summary>
        public const string PreviousStateIsNull = "Previous state is null";
        
        /// <summary>Error message for controller grammar type in regex context.</summary>
        public const string ControllerGrammarTypeInRegex = "Controller GrammarType in regex";
    }
}