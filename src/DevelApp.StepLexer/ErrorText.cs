using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Contains error message constants used throughout the StepLexer for consistent error reporting
    /// </summary>
    public class ErrorText
    {
        /// <summary>
        /// Error message for when a grouping end is encountered without a matching grouping start
        /// </summary>
        public const string GroupingEndWithoutGroupingStart = "Grouping End without Grouping Start";
        
        /// <summary>
        /// Error message for when a grouping start lookbehind is missing positive or negative specifier
        /// </summary>
        public const string GroupingStartLookbehindMissingPositiveOrNegative = "Grouping Start Lookbehind missing Positive or Negative";
        
        /// <summary>
        /// Error message for when exit context is encountered before pattern end
        /// </summary>
        public const string ExitContextBeforePatternEnd = "Exit Context before Pattern End";
        
        /// <summary>
        /// Error message for when a named backreference is missing the start group name (&lt;)
        /// </summary>
        public const string NamedBackreferenceMissingStartGroupName = "Named Backreference missing Start Group Name (<)";
        
        /// <summary>
        /// Error message for when a plus sign appears as the first character in a pattern
        /// </summary>
        public const string PlusSignAsFirstCharInPattern = "Plus sign as first char in pattern";
        
        /// <summary>
        /// Error message for when end of line appears as the first character in a pattern
        /// </summary>
        public const string EndOfLineAsFirstCharInPattern = "End Of Line as first char in pattern";
        
        /// <summary>
        /// Error message for when a right curly bracket is encountered without a matching left curly bracket
        /// </summary>
        public const string RightCurlyBracketWithoutMatchingLeftCurlyBracket = "Right Curly Bracket without matching Left Curly Bracket";
        
        /// <summary>
        /// Error message for when a left curly bracket appears as the first character in a pattern
        /// </summary>
        public const string LeftCurlyBracketAsFirstCharInPattern = "Left Curly Bracket as first char in pattern";
        
        /// <summary>
        /// Error message for when a right square bracket is encountered without a matching left square bracket
        /// </summary>
        public const string RightSquareBracketWithoutMatchingLeftSquareBracket = "Right Square Bracket without matching Left Square Bracket";
        
        /// <summary>
        /// Error message for when an asterisk appears as the first character in a pattern
        /// </summary>
        public const string AsterixAsFirstCharInPattern = "Asterix as first char in pattern";
        
        /// <summary>
        /// Error message for when a question mark appears as the first character in a pattern
        /// </summary>
        public const string QuestionMarkAsFirstCharInPattern = "Question Mark as first char in pattern";
        
        /// <summary>
        /// Error message for when a character range has no end value
        /// </summary>
        public const string CharacterRangeHasNoEndValue = "Character Range has no end value";
        
        /// <summary>
        /// Error message for when end of stream is reached before pattern is finished
        /// </summary>
        public const string EndOfStreamBeforeCharFound = "End Of Stream before pattern finished";
        
        /// <summary>
        /// Error message for when empty curly braces are encountered
        /// </summary>
        public const string EmptyCurlyBraces = "Empty Curly Braces";
        
        /// <summary>
        /// Error message for when minimum repetitions cannot be parsed
        /// </summary>
        public const string CouldNotParseMinRepetitions = "Could not parse min repetitions";
        
        /// <summary>
        /// Error message for when maximum repetitions cannot be parsed
        /// </summary>
        public const string CouldNotParseMaxRepetitions = "Could not parse max repetitions";
        
        /// <summary>
        /// Error message for when a character is escaped without being expected to be
        /// </summary>
        public const string CharacterEscapedWithoutBeingExpectedTo = "Character escaped without being expected to";
        
        /// <summary>
        /// Error message for when a character class is escaped without being expected to be
        /// </summary>
        public const string CharacterClassEscapedWithoutBeingExpectedTo = "Character Class escaped without being expected to";
        
        /// <summary>
        /// Error message for when a character class is empty
        /// </summary>
        public const string CharacterClassEmpty = "Character Class empty";
        
        /// <summary>
        /// Error message for when a grouping is expected to have a specifier after question mark
        /// </summary>
        public const string GroupingExpectedSpecifierAfterQuestionMark = "Grouping expected specifier after Question Mark";
        
        /// <summary>
        /// Error message for when a named group cannot be empty
        /// </summary>
        public const string NamedGroupCannotBeEmpty = "Named Group cannot be empty";
        
        /// <summary>
        /// Error message for when looking up a group name from number results in too high a number
        /// </summary>
        public const string LookupGroupNameFromNumberTooHighNumber = "Lookup Group Name from number too high number";
        
        /// <summary>
        /// Error message for when a specified group name does not exist
        /// </summary>
        public const string SpecifiedGroupNameDoesNotExist = "Specified Group Name does not exist";
        
        /// <summary>
        /// Error message for when trying to create a new grammar transition in regex context
        /// </summary>
        public const string TryingToCreateNewGrammarTransitionInRegex = "Trying to create new Grammar Transition in regex";
        
        /// <summary>
        /// Error message for when trying to create a new regex transition in grammar context
        /// </summary>
        public const string TryingToCreateNewRegexTransitionInGrammar = "Trying to create new Regex Transition in grammar";
        
        /// <summary>
        /// Error message for when the previous state is null
        /// </summary>
        public const string PreviousStateIsNull = "Previous state is null";
        
        /// <summary>
        /// Error message for when controller grammar type is used in regex context
        /// </summary>
        public const string ControllerGrammarTypeInRegex = "Controller GrammarType in regex";
    }
}