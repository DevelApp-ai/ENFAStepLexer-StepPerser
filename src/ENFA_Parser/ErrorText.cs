using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENFA_Parser
{
    /// <summary>
    /// Provides error message constants for ENFA parser operations.
    /// Contains standardized error messages used throughout the parsing process.
    /// </summary>
    public class ErrorText
    {
        /// <summary>
        /// Error message for encountering a grouping end without a corresponding grouping start.
        /// </summary>
        public const string GroupingEndWithoutGroupingStart = "Grouping End without Grouping Start";
        
        /// <summary>
        /// Error message for grouping start lookbehind missing positive or negative specifier.
        /// </summary>
        public const string GroupingStartLookbehindMissingPositiveOrNegative = "Grouping Start Lookbehind missing Positive or Negative";
        
        /// <summary>
        /// Error message for encountering exit context before pattern end.
        /// </summary>
        public const string ExitContextBeforePatternEnd = "Exit Context before Pattern End";
        
        /// <summary>
        /// Error message for named backreference missing start group name (&lt;).
        /// </summary>
        public const string NamedBackreferenceMissingStartGroupName = "Named Backreference missing Start Group Name (<)";
        
        /// <summary>
        /// Error message for plus sign appearing as the first character in a pattern.
        /// </summary>
        public const string PlusSignAsFirstCharInPattern = "Plus sign as first char in pattern";
        
        /// <summary>
        /// Error message for end of line appearing as the first character in a pattern.
        /// </summary>
        public const string EndOfLineAsFirstCharInPattern = "End Of Line as first char in pattern";
        
        /// <summary>
        /// Error message for right curly bracket without matching left curly bracket.
        /// </summary>
        public const string RightCurlyBracketWithoutMatchingLeftCurlyBracket = "Right Curly Bracket without matching Left Curly Bracket";
        
        /// <summary>
        /// Error message for left curly bracket appearing as the first character in a pattern.
        /// </summary>
        public const string LeftCurlyBracketAsFirstCharInPattern = "Left Curly Bracket as first char in pattern";
        
        /// <summary>
        /// Error message for right square bracket without matching left square bracket.
        /// </summary>
        public const string RightSquareBracketWithoutMatchingLeftSquareBracket = "Right Square Bracket without matching Left Square Bracket";
        
        /// <summary>
        /// Error message for asterisk appearing as the first character in a pattern.
        /// </summary>
        public const string AsterixAsFirstCharInPattern = "Asterix as first char in pattern";
        
        /// <summary>
        /// Error message for question mark appearing as the first character in a pattern.
        /// </summary>
        public const string QuestionMarkAsFirstCharInPattern = "Question Mark as first char in pattern";
        
        /// <summary>
        /// Error message for character range missing an end value.
        /// </summary>
        public const string CharacterRangeHasNoEndValue = "Character Range has no end value";
        
        /// <summary>
        /// Error message for reaching end of stream before pattern is finished.
        /// </summary>
        public const string EndOfStreamBeforeCharFound = "End Of Stream before pattern finished";
        
        /// <summary>
        /// Error message for empty curly braces in pattern.
        /// </summary>
        public const string EmptyCurlyBraces = "Empty Curly Braces";
        
        /// <summary>
        /// Error message for inability to parse minimum repetitions value.
        /// </summary>
        public const string CouldNotParseMinRepetitions = "Could not parse min repetitions";
        
        /// <summary>
        /// Error message for inability to parse maximum repetitions value.
        /// </summary>
        public const string CouldNotParseMaxRepetitions = "Could not parse max repetitions";
        
        /// <summary>
        /// Error message for character being escaped without expectation.
        /// </summary>
        public const string CharacterEscapedWithoutBeingExpectedTo = "Character escaped without being expected to";
        
        /// <summary>
        /// Error message for character class being escaped without expectation.
        /// </summary>
        public const string CharacterClassEscapedWithoutBeingExpectedTo = "Character Class escaped without being expected to";
        
        /// <summary>
        /// Error message for empty character class.
        /// </summary>
        public const string CharacterClassEmpty = "Character Class empty";
        
        /// <summary>
        /// Error message for grouping expecting specifier after question mark.
        /// </summary>
        public const string GroupingExpectedSpecifierAfterQuestionMark = "Grouping expected specifier after Question Mark";
        
        /// <summary>
        /// Error message for named group being empty.
        /// </summary>
        public const string NamedGroupCannotBeEmpty = "Named Group cannot be empty";
        
        /// <summary>
        /// Error message for group name lookup number being too high.
        /// </summary>
        public const string LookupGroupNameFromNumberTooHighNumber = "Lookup Group Name from number too high number";
        
        /// <summary>
        /// Error message for specified group name not existing.
        /// </summary>
        public const string SpecifiedGroupNameDoesNotExist = "Specified Group Name does not exist";
        
        /// <summary>
        /// Error message for trying to create new grammar transition in regex context.
        /// </summary>
        public const string TryingToCreateNewGrammarTransitionInRegex = "Trying to create new Grammar Transition in regex";
        
        /// <summary>
        /// Error message for trying to create new regex transition in grammar context.
        /// </summary>
        public const string TryingToCreateNewRegexTransitionInGrammar = "Trying to create new Regex Transition in grammar";
        
        /// <summary>
        /// Error message for previous state being null.
        /// </summary>
        public const string PreviousStateIsNull = "Previous state is null";
        
        /// <summary>
        /// Error message for controller grammar type appearing in regex context.
        /// </summary>
        public const string ControllerGrammarTypeInRegex = "Controller GrammarType in regex";
    }
}