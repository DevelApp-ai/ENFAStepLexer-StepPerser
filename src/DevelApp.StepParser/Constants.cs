using System.Text;

namespace DevelApp.StepParser
{
    /// <summary>
    /// Static constants for StepParser operations including character definitions, 
    /// stream handling parameters, and special pattern identifiers.
    /// </summary>
    public static class Constants
    {
        /// <summary>Buffer size for stream operations (512 bytes).</summary>
        public const int BufferSize = 512;
        
        /// <summary>Whether to detect stream encoding from byte order marks.</summary>
        public const bool DetectStreamEncodingFromByteOrderMarks = true;
        
        /// <summary>Default stream encoding set to UTF-8.</summary>
        public static Encoding StreamEncoding = Encoding.UTF8;
        
        /// <summary>Null character (Unicode U+0000).</summary>
        public const char NullChar = (char)0x0000;
        
        /// <summary>Single quote character (Unicode U+0027) - '.</summary>
        public const char SingleQuote = (char)0x0027;
        
        /// <summary>Double quote character (Unicode U+0022) - ".</summary>
        public const char DoubleQuote = (char)0x0022;
        
        /// <summary>Backslash character (Unicode U+005C) - \.</summary>
        public const char Backslash = (char)0x005C;
        
        /// <summary>Alert character (Unicode U+0007) - bell/beep.</summary>
        public const char Alert = (char)0x0007;
        
        /// <summary>Backspace character (Unicode U+0008).</summary>
        public const char Backspace = (char)0x0008;
        
        /// <summary>Form feed character (Unicode U+000C).</summary>
        public const char FormFeed = (char)0x000C;
        
        /// <summary>Line feed character (Unicode U+000A) - \n.</summary>
        public const char LineFeed = (char)0x000A;
        
        /// <summary>Carriage return character (Unicode U+000D) - \r.</summary>
        public const char CarriageReturn = (char)0x000D;
        
        /// <summary>Horizontal tab character (Unicode U+0009) - \t.</summary>
        public const char HorizontalTab = (char)0x0009;
        
        /// <summary>Vertical tab character (Unicode U+000B) - \v.</summary>
        public const char VerticalTab = (char)0x000B;
        
        /// <summary>Underscore character (Unicode U+005F) - _.</summary>
        public const char Underscore = (char)0x005F;
        
        /// <summary>Circumflex accent character (Unicode U+005E) - ^.</summary>
        public const char CircumflexAccent = (char)0x005E;
        
        /// <summary>Dollar sign character (Unicode U+0024) - $.</summary>
        public const char DollarSign = (char)0x0024;
        
        /// <summary>Left curly bracket character (Unicode U+007B) - {.</summary>
        public const char LeftCurlyBracket = (char)0x007B;
        
        /// <summary>Right curly bracket character (Unicode U+007D) - }.</summary>
        public const char RightCurlyBracket = (char)0x007D;
        
        /// <summary>Left square bracket character (Unicode U+005B) - [.</summary>
        public const char LeftSquareBracket = (char)0x005B;
        
        /// <summary>Right square bracket character (Unicode U+005D) - ].</summary>
        public const char RightSquareBracket = (char)0x005D;
        
        /// <summary>Left parenthesis character (Unicode U+0028) - (.</summary>
        public const char LeftParanthesis = (char)0x0028;
        
        /// <summary>Right parenthesis character (Unicode U+0029) - ).</summary>
        public const char RightParanthesis = (char)0x0029;
        
        /// <summary>Vertical line character (Unicode U+007C) - |.</summary>
        public const char VerticalLine = (char)0x007C;
        
        /// <summary>Less than sign character (Unicode U+003C) - &lt;.</summary>
        public const char LessThanSign = (char)0x003C;
        
        /// <summary>Greater than sign character (Unicode U+003E) - &gt;.</summary>
        public const char GreaterThanSign = (char)0x003E;
        
        /// <summary>Colon character (Unicode U+003A) - :.</summary>
        public const char Colon = (char)0x003A;
        
        /// <summary>Full stop character (Unicode U+002E) - . (period).</summary>
        public const char FullStop = (char)0x002E;
        
        /// <summary>Comma character (Unicode U+002C) - ,.</summary>
        public const char Comma = (char)0x002C;
        
        /// <summary>Plus sign character (Unicode U+002B) - +.</summary>
        public const char PlusSign = (char)0x002B;
        
        /// <summary>Hyphen minus sign character (Unicode U+002D) - -.</summary>
        public const char HyphenMinusSign = (char)0x002D;
        
        /// <summary>Asterisk character (Unicode U+002A) - *.</summary>
        public const char Asterisk = (char)0x002A;
        
        /// <summary>Question mark character (Unicode U+003F) - ?.</summary>
        public const char QuestionMark = (char)0x003F;
        
        /// <summary>Equals sign character (Unicode U+003D) - =.</summary>
        public const char EqualsSign = (char)0x003D;
        
        /// <summary>Exclamation mark character (Unicode U+0021) - !.</summary>
        public const char ExclamationMark = (char)0x0021;
        
        /// <summary>Escape character (Unicode U+001B).</summary>
        public const char Escape = (char)0x001B;
        
        /// <summary>Space character (Unicode U+0020).</summary>
        public const char Space = (char)0x0020;

        /* Renamed for regex */
        /// <summary>Exit context character - alias for DoubleQuote in regex patterns.</summary>
        public const char ExitContext = DoubleQuote;
        
        /// <summary>New line character - alias for LineFeed in regex patterns.</summary>
        public const char NewLine = LineFeed;
        
        /// <summary>All but new line character - alias for FullStop in regex patterns.</summary>
        public const char AllButNewLine = FullStop;
        
        /// <summary>Start of line character - alias for CircumflexAccent in regex patterns.</summary>
        public const char StartOfLine = CircumflexAccent;
        
        /// <summary>End of line character - alias for DollarSign in regex patterns.</summary>
        public const char EndOfLine = DollarSign;
        
        /// <summary>Grouping end character - alias for RightParanthesis in regex patterns.</summary>
        public const char GroupingEnd = RightParanthesis;
        
        /// <summary>Grouping start character - alias for LeftParanthesis in regex patterns.</summary>
        public const char GroupingStart = LeftParanthesis;

        /* Hierarchy Printing */
        /// <summary>Bend pipe character (Unicode U+2514) - └ for tree visualization.</summary>
        public const char BendPipe = '\u2514';
        
        /// <summary>T-pipe character (Unicode U+251C) - ├ for tree visualization.</summary>
        public const char TPipe = '\u251C';
        
        /// <summary>Horizontal pipe character (Unicode U+2500) - ─ for tree visualization.</summary>
        public const char HorizontalPipe = '\u2500';
        
        /// <summary>Horizontal T-pipe character (Unicode U+252C) - ┬ for tree visualization.</summary>
        public const char HorizontalTPipe = '\u252C';
        
        /// <summary>Vertical pipe character (Unicode U+2502) - │ for tree visualization.</summary>
        public const char VerticalPipe = '\u2502';

        /* Special Patterns */
        /// <summary>Special pattern identifier indicating the end of an ENFA pattern.</summary>
        public const string ENFA_PatternEnd = "ENFA_PatternEnd";
    }
}