using System;
using ENFA_Parser.Core;

namespace ENFAStepLexer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ENFAStepLexer-StepParser Pattern Parser Demo");
            Console.WriteLine("Zero-Copy UTF-8 Architecture with Two-Phase Parsing");
            Console.WriteLine("=====================================================");
            Console.WriteLine();

            PatternParserDemo.RunDemo();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}