using System.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Constants used throughout the StepLexer for character definitions, encoding settings, and special patterns
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default buffer size for reading input streams
        /// </summary>
        public const int BufferSize = 512;
        
        /// <summary>
        /// Whether to detect stream encoding from byte order marks
        /// </summary>
        public const bool DetectStreamEncodingFromByteOrderMarks = true;
        
        /// <summary>
        /// Default UTF-8 encoding for stream processing
        /// </summary>
        public static Encoding StreamEncoding = Encoding.UTF8;
        
        /// <summary>
        /// Null character (0x0000)
        /// </summary>
        public const char NullChar = (char)0x0000;
        
        /// <summary>
        /// Single quote character (0x0027)
        /// </summary>
        public const char SingleQuote = (char)0x0027;
        
        /// <summary>
        /// Double quote character (0x0022)
        /// </summary>
        public const char DoubleQuote = (char)0x0022;
        
        /// <summary>
        /// Backslash character (0x005C)
        /// </summary>
        public const char Backslash = (char)0x005C;
        
        /// <summary>
        /// Alert/bell character (0x0007)
        /// </summary>
        public const char Alert = (char)0x0007;
        
        /// <summary>
        /// Backspace character (0x0008)
        /// </summary>
        public const char Backspace = (char)0x0008;
        
        /// <summary>
        /// Form feed character (0x000C)
        /// </summary>
        public const char FormFeed = (char)0x000C;
        
        /// <summary>
        /// Line feed character (0x000A)
        /// </summary>
        public const char LineFeed = (char)0x000A;
        
        /// <summary>
        /// Carriage return character (0x000D)
        /// </summary>
        public const char CarriageReturn = (char)0x000D;
        
        /// <summary>
        /// Horizontal tab character (0x0009)
        /// </summary>
        public const char HorizontalTab = (char)0x0009;
        
        /// <summary>
        /// Vertical tab character (0x000B)
        /// </summary>
        public const char VerticalTab = (char)0x000B;
        
        /// <summary>
        /// Underscore character (0x005F)
        /// </summary>
        public const char Underscore = (char)0x005F;
        
        /// <summary>
        /// Circumflex accent character (0x005E) - used for start of line in regex
        /// </summary>
        public const char CircumflexAccent = (char)0x005E;
        
        /// <summary>
        /// Dollar sign character (0x0024) - used for end of line in regex
        /// </summary>
        public const char DollarSign = (char)0x0024;
        
        /// <summary>
        /// Left curly bracket character (0x007B)
        /// </summary>
        public const char LeftCurlyBracket = (char)0x007B;
        
        /// <summary>
        /// Right curly bracket character (0x007D)
        /// </summary>
        public const char RightCurlyBracket = (char)0x007D;
        
        /// <summary>
        /// Left square bracket character (0x005B)
        /// </summary>
        public const char LeftSquareBracket = (char)0x005B;
        
        /// <summary>
        /// Right square bracket character (0x005D)
        /// </summary>
        public const char RightSquareBracket = (char)0x005D;
        
        /// <summary>
        /// Left parenthesis character (0x0028)
        /// </summary>
        public const char LeftParanthesis = (char)0x0028;
        
        /// <summary>
        /// Right parenthesis character (0x0029)
        /// </summary>
        public const char RightParanthesis = (char)0x0029;
        
        /// <summary>
        /// Vertical line character (0x007C) - used for alternation in regex
        /// </summary>
        public const char VerticalLine = (char)0x007C;
        
        /// <summary>
        /// Less than sign character (0x003C)
        /// </summary>
        public const char LessThanSign = (char)0x003C;
        
        /// <summary>
        /// Greater than sign character (0x003E)
        /// </summary>
        public const char GreaterThanSign = (char)0x003E;
        
        /// <summary>
        /// Colon character (0x003A)
        /// </summary>
        public const char Colon = (char)0x003A;
        
        /// <summary>
        /// Full stop/period character (0x002E) - used for any character in regex
        /// </summary>
        public const char FullStop = (char)0x002E;
        
        /// <summary>
        /// Comma character (0x002C)
        /// </summary>
        public const char Comma = (char)0x002C;
        
        /// <summary>
        /// Plus sign character (0x002B) - used for one-or-more quantifier in regex
        /// </summary>
        public const char PlusSign = (char)0x002B;
        
        /// <summary>
        /// Hyphen minus sign character (0x002D)
        /// </summary>
        public const char HyphenMinusSign = (char)0x002D;
        
        /// <summary>
        /// Asterisk character (0x002A) - used for zero-or-more quantifier in regex
        /// </summary>
        public const char Asterisk = (char)0x002A;
        
        /// <summary>
        /// Question mark character (0x003F) - used for zero-or-one quantifier in regex
        /// </summary>
        public const char QuestionMark = (char)0x003F;
        
        /// <summary>
        /// Equals sign character (0x003D)
        /// </summary>
        public const char EqualsSign = (char)0x003D;
        
        /// <summary>
        /// Exclamation mark character (0x0021)
        /// </summary>
        public const char ExclamationMark = (char)0x0021;
        
        /// <summary>
        /// Escape character (0x001B)
        /// </summary>
        public const char Escape = (char)0x001B;
        
        /// <summary>
        /// Space character (0x0020)
        /// </summary>
        public const char Space = (char)0x0020;

        /* Renamed for regex */
        /// <summary>
        /// Exit context character - alias for DoubleQuote
        /// </summary>
        public const char ExitContext = DoubleQuote;
        
        /// <summary>
        /// New line character - alias for LineFeed
        /// </summary>
        public const char NewLine = LineFeed;
        
        /// <summary>
        /// Any character except new line - alias for FullStop
        /// </summary>
        public const char AllButNewLine = FullStop;
        
        /// <summary>
        /// Start of line anchor - alias for CircumflexAccent
        /// </summary>
        public const char StartOfLine = CircumflexAccent;
        
        /// <summary>
        /// End of line anchor - alias for DollarSign
        /// </summary>
        public const char EndOfLine = DollarSign;
        
        /// <summary>
        /// Grouping end character - alias for RightParanthesis
        /// </summary>
        public const char GroupingEnd = RightParanthesis;
        
        /// <summary>
        /// Grouping start character - alias for LeftParanthesis
        /// </summary>
        public const char GroupingStart = LeftParanthesis;

        /* Hierarchy Printing */
        /// <summary>
        /// Unicode bend pipe character for tree visualization (└)
        /// </summary>
        public const char BendPipe = '\u2514';
        
        /// <summary>
        /// Unicode T-pipe character for tree visualization (├)
        /// </summary>
        public const char TPipe = '\u251C';
        
        /// <summary>
        /// Unicode horizontal pipe character for tree visualization (─)
        /// </summary>
        public const char HorizontalPipe = '\u2500';
        
        /// <summary>
        /// Unicode horizontal T-pipe character for tree visualization (┬)
        /// </summary>
        public const char HorizontalTPipe = '\u252C';
        
        /// <summary>
        /// Unicode vertical pipe character for tree visualization (│)
        /// </summary>
        public const char VerticalPipe = '\u2502';

        /* Special Patterns */
        /// <summary>
        /// Special pattern marker indicating the end of an ENFA pattern
        /// </summary>
        public const string ENFA_PatternEnd = "ENFA_PatternEnd";
    }
}