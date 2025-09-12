using System.Text;

namespace ENFA_Parser
{
    /// <summary>
    /// Provides commonly used character constants and configuration values for ENFA parsing operations.
    /// Contains Unicode character constants, buffer sizes, and special pattern identifiers.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default buffer size for stream operations in the ENFA parser.
        /// </summary>
        public const int BufferSize = 512;
        
        /// <summary>
        /// Indicates whether to detect stream encoding from byte order marks (BOMs).
        /// </summary>
        public const bool DetectStreamEncodingFromByteOrderMarks = true;
        
        /// <summary>
        /// Default encoding used for stream operations (UTF-8).
        /// </summary>
        public static Encoding StreamEncoding = Encoding.UTF8;
        
        /// <summary>
        /// Null character (Unicode U+0000).
        /// </summary>
        public const char NullChar = (char)0x0000;
        
        /// <summary>
        /// Single quote character (Unicode U+0027).
        /// </summary>
        public const char SingleQuote = (char)0x0027;
        
        /// <summary>
        /// Double quote character (Unicode U+0022).
        /// </summary>
        public const char DoubleQuote = (char)0x0022;
        
        /// <summary>
        /// Backslash character (Unicode U+005C).
        /// </summary>
        public const char Backslash = (char)0x005C;
        
        /// <summary>
        /// Alert/bell character (Unicode U+0007).
        /// </summary>
        public const char Alert = (char)0x0007;
        
        /// <summary>
        /// Backspace character (Unicode U+0008).
        /// </summary>
        public const char Backspace = (char)0x0008;
        
        /// <summary>
        /// Form feed character (Unicode U+000C).
        /// </summary>
        public const char FormFeed = (char)0x000C;
        
        /// <summary>
        /// Line feed/newline character (Unicode U+000A).
        /// </summary>
        public const char LineFeed = (char)0x000A;
        
        /// <summary>
        /// Carriage return character (Unicode U+000D).
        /// </summary>
        public const char CarriageReturn = (char)0x000D;
        
        /// <summary>
        /// Horizontal tab character (Unicode U+0009).
        /// </summary>
        public const char HorizontalTab = (char)0x0009;
        
        /// <summary>
        /// Vertical tab character (Unicode U+000B).
        /// </summary>
        public const char VerticalTab = (char)0x000B;
        
        /// <summary>
        /// Underscore character (Unicode U+005F).
        /// </summary>
        public const char Underscore = (char)0x005F;
        
        /// <summary>
        /// Circumflex accent character (Unicode U+005E).
        /// </summary>
        public const char CircumflexAccent = (char)0x005E;
        
        /// <summary>
        /// Dollar sign character (Unicode U+0024).
        /// </summary>
        public const char DollarSign = (char)0x0024;
        
        /// <summary>
        /// Left curly bracket character (Unicode U+007B).
        /// </summary>
        public const char LeftCurlyBracket = (char)0x007B;
        
        /// <summary>
        /// Right curly bracket character (Unicode U+007D).
        /// </summary>
        public const char RightCurlyBracket = (char)0x007D;
        
        /// <summary>
        /// Left square bracket character (Unicode U+005B).
        /// </summary>
        public const char LeftSquareBracket = (char)0x005B;
        
        /// <summary>
        /// Right square bracket character (Unicode U+005D).
        /// </summary>
        public const char RightSquareBracket = (char)0x005D;
        
        /// <summary>
        /// Left parenthesis character (Unicode U+0028).
        /// </summary>
        public const char LeftParanthesis = (char)0x0028;
        
        /// <summary>
        /// Right parenthesis character (Unicode U+0029).
        /// </summary>
        public const char RightParanthesis = (char)0x0029;
        
        /// <summary>
        /// Vertical line/pipe character (Unicode U+007C).
        /// </summary>
        public const char VerticalLine = (char)0x007C;
        
        /// <summary>
        /// Less-than sign character (Unicode U+003C).
        /// </summary>
        public const char LessThanSign = (char)0x003C;
        
        /// <summary>
        /// Greater-than sign character (Unicode U+003E).
        /// </summary>
        public const char GreaterThanSign = (char)0x003E;
        
        /// <summary>
        /// Colon character (Unicode U+003A).
        /// </summary>
        public const char Colon = (char)0x003A;
        
        /// <summary>
        /// Full stop/period character (Unicode U+002E).
        /// </summary>
        public const char FullStop = (char)0x002E;
        
        /// <summary>
        /// Comma character (Unicode U+002C).
        /// </summary>
        public const char Comma = (char)0x002C;
        
        /// <summary>
        /// Plus sign character (Unicode U+002B).
        /// </summary>
        public const char PlusSign = (char)0x002B;
        
        /// <summary>
        /// Hyphen-minus sign character (Unicode U+002D).
        /// </summary>
        public const char HyphenMinusSign = (char)0x002D;
        
        /// <summary>
        /// Asterisk character (Unicode U+002A).
        /// </summary>
        public const char Asterisk = (char)0x002A;
        
        /// <summary>
        /// Question mark character (Unicode U+003F).
        /// </summary>
        public const char QuestionMark = (char)0x003F;
        
        /// <summary>
        /// Equals sign character (Unicode U+003D).
        /// </summary>
        public const char EqualsSign = (char)0x003D;
        
        /// <summary>
        /// Exclamation mark character (Unicode U+0021).
        /// </summary>
        public const char ExclamationMark = (char)0x0021;
        
        /// <summary>
        /// Escape character (Unicode U+001B).
        /// </summary>
        public const char Escape = (char)0x001B;
        
        /// <summary>
        /// Space character (Unicode U+0020).
        /// </summary>
        public const char Space = (char)0x0020;

        /* Renamed for regex */
        /// <summary>
        /// Exit context character for regex patterns (equivalent to DoubleQuote).
        /// </summary>
        public const char ExitContext = DoubleQuote;
        
        /// <summary>
        /// New line character for regex patterns (equivalent to LineFeed).
        /// </summary>
        public const char NewLine = LineFeed;
        
        /// <summary>
        /// All characters except new line for regex patterns (equivalent to FullStop).
        /// </summary>
        public const char AllButNewLine = FullStop;
        
        /// <summary>
        /// Start of line anchor for regex patterns (equivalent to CircumflexAccent).
        /// </summary>
        public const char StartOfLine = CircumflexAccent;
        
        /// <summary>
        /// End of line anchor for regex patterns (equivalent to DollarSign).
        /// </summary>
        public const char EndOfLine = DollarSign;
        
        /// <summary>
        /// Grouping end character for regex patterns (equivalent to RightParanthesis).
        /// </summary>
        public const char GroupingEnd = RightParanthesis;
        
        /// <summary>
        /// Grouping start character for regex patterns (equivalent to LeftParanthesis).
        /// </summary>
        public const char GroupingStart = LeftParanthesis;

        /* Hierarchy Printing */
        /// <summary>
        /// Unicode character for bend pipe in hierarchy printing (U+2514).
        /// </summary>
        public const char BendPipe = '\u2514';
        
        /// <summary>
        /// Unicode character for T-pipe in hierarchy printing (U+251C).
        /// </summary>
        public const char TPipe = '\u251C';
        
        /// <summary>
        /// Unicode character for horizontal pipe in hierarchy printing (U+2500).
        /// </summary>
        public const char HorizontalPipe = '\u2500';
        
        /// <summary>
        /// Unicode character for horizontal T-pipe in hierarchy printing (U+252C).
        /// </summary>
        public const char HorizontalTPipe = '\u252C';
        
        /// <summary>
        /// Unicode character for vertical pipe in hierarchy printing (U+2502).
        /// </summary>
        public const char VerticalPipe = '\u2502';

        /* Special Patterns */
        /// <summary>
        /// Special pattern identifier indicating the end of an ENFA pattern.
        /// </summary>
        public const string ENFA_PatternEnd = "ENFA_PatternEnd";
    }
}