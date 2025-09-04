using System;
using System.IO;
using System.Text;
using ENFA_Parser;

namespace ENFAStepLexer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ENFAStepLexer-StepParser Demo");
            Console.WriteLine("Enhanced PCRE2 Regex Support");
            Console.WriteLine("============================");
            Console.WriteLine();

            // Demonstrate various regex features
            TestRegexFeatures();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void TestRegexFeatures()
        {
            // Test basic patterns
            TestPattern("abc", "Basic literal matching");
            TestPattern("[a-z]+", "Character class and quantifier");
            TestPattern("\\d{2,4}", "Digit class with quantifier");
            TestPattern("\\w+@\\w+\\.\\w+", "Email-like pattern");
            
            // Test new PCRE2 features
            TestPattern("\\A\\w+\\z", "String anchors (\\A and \\z)");
            TestPattern("\\x{41}\\x{42}", "Unicode code points");
            TestPattern("\\cA\\cB", "Control characters");
            TestPattern("\\p{L}+", "Unicode property (Letters)");
            TestPattern("[[:alpha:]]+", "POSIX character class");
            TestPattern("\\R", "Unicode newline");
            
            // Test groups and assertions
            TestPattern("(?:test)", "Non-capturing group");
            TestPattern("(?<name>\\w+)", "Named group");
            TestPattern("test(?=ing)", "Positive lookahead");
            TestPattern("(?<!pre)test", "Negative lookbehind");
        }

        static void TestPattern(string pattern, string description)
        {
            Console.WriteLine($"Testing: {description}");
            Console.WriteLine($"Pattern: \"{pattern}\"");
            
            try
            {
                // Create ENFA controller for regex parsing
                var controller = new ENFA_Controller(ParserType.Regex);
                
                // Create a stream reader from the pattern
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pattern + "\"")))
                using (var reader = new StreamReader(stream))
                {
                    // Tokenize the pattern
                    bool success = controller.Tokenizer.Tokenize("test_pattern", reader);
                    
                    if (success)
                    {
                        Console.WriteLine("✅ Pattern compiled successfully");
                        
                        // Print the ENFA hierarchy
                        Console.WriteLine("ENFA Structure:");
                        string hierarchy = controller.PrintHierarchy;
                        Console.WriteLine(hierarchy);
                    }
                    else
                    {
                        Console.WriteLine("❌ Pattern compilation failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }
    }
}