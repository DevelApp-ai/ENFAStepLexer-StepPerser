using System;
using DevelApp.StepLexer;
using DevelApp.StepParser;

namespace ENFAStepLexer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ENFAStepLexer-StepParser Pattern Parser Demo");
            Console.WriteLine("Zero-Copy UTF-8 Architecture with Step-Based Parsing");
            Console.WriteLine("====================================================");
            Console.WriteLine();

            // Original PatternParser Demo
            PatternParserDemo.RunDemo();
            
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("GrammarForge Step-Parser Demo");
            Console.WriteLine("Real-Time Parsing with Location-Based Operations");
            Console.WriteLine(new string('=', 60));
            
            // New Step-Parser Demo
            StepParserDemo.RunDemo();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}