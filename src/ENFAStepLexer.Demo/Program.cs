using System;
using ENFA_Parser.vNext;

namespace ENFAStepLexer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ENFAStepLexer-StepParser vNext Demo");
            Console.WriteLine("Zero-Copy UTF-8 Architecture with Two-Phase Parsing");
            Console.WriteLine("=====================================================");
            Console.WriteLine();

            vNextDemo.RunDemo();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}