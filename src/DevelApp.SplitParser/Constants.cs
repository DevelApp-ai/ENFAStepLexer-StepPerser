using System.Text;

namespace DevelApp.SplitParser
{
    public static class Constants
    {
        public const int BufferSize = 512;
        public const bool DetectStreamEncodingFromByteOrderMarks = true;
        public static Encoding StreamEncoding = Encoding.UTF8;
        public const char NullChar = (char)0x0000;
        public const char SingleQuote = (char)0x0027;
        public const char DoubleQuote = (char)0x0022;
        public const char Backslash = (char)0x005C;
        public const char Alert = (char)0x0007;
        public const char Backspace = (char)0x0008;
        public const char FormFeed = (char)0x000C;
        public const char LineFeed = (char)0x000A;
        public const char CarriageReturn = (char)0x000D;
        public const char HorizontalTab = (char)0x0009;
        public const char VerticalTab = (char)0x000B;
        public const char Underscore = (char)0x005F;
        public const char CircumflexAccent = (char)0x005E;
        public const char DollarSign = (char)0x0024;
        public const char LeftCurlyBracket = (char)0x007B;
        public const char RightCurlyBracket = (char)0x007D;
        public const char LeftSquareBracket = (char)0x005B;
        public const char RightSquareBracket = (char)0x005D;
        public const char LeftParanthesis = (char)0x0028;
        public const char RightParanthesis = (char)0x0029;
        public const char VerticalLine = (char)0x007C;
        public const char LessThanSign = (char)0x003C;
        public const char GreaterThanSign = (char)0x003E;
        public const char Colon = (char)0x003A;
        public const char FullStop = (char)0x002E;
        public const char Comma = (char)0x002C;
        public const char PlusSign = (char)0x002B;
        public const char HyphenMinusSign = (char)0x002D;
        public const char Asterisk = (char)0x002A;
        public const char QuestionMark = (char)0x003F;
        public const char EqualsSign = (char)0x003D;
        public const char ExclamationMark = (char)0x0021;
        public const char Escape = (char)0x001B;
        public const char Space = (char)0x0020;

        /* Renamed for regex */
        public const char ExitContext = DoubleQuote;
        public const char NewLine = LineFeed;
        public const char AllButNewLine = FullStop;
        public const char StartOfLine = CircumflexAccent;
        public const char EndOfLine = DollarSign;
        public const char GroupingEnd = RightParanthesis;
        public const char GroupingStart = LeftParanthesis;

        /* Hierarchy Printing */
        public const char BendPipe = '\u2514';
        public const char TPipe = '\u251C';
        public const char HorizontalPipe = '\u2500';
        public const char HorizontalTPipe = '\u252C';
        public const char VerticalPipe = '\u2502';

        /* Special Patterns */
        public const string ENFA_PatternEnd = "ENFA_PatternEnd";
    }
}