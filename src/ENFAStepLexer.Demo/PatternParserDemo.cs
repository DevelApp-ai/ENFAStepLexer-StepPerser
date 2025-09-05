using System;
using System.Linq;
using System.Text;
using ENFA_Parser.Core;

namespace ENFAStepLexer.Demo
{
    /// <summary>
    /// Demonstrates the modern architecture with zero-copy parsing and two-phase processing
    /// </summary>
    public static class PatternParserDemo
    {
        public static void RunDemo()
        {
            Console.WriteLine("=== ENFA Pattern Parser Demo ===");
            Console.WriteLine("Zero-copy parsing with UTF-8 optimization and two-phase processing");
            Console.WriteLine();
            
            // Test patterns with increasing complexity
            var testPatterns = new[]
            {
                @"\d{2,4}",                    // Simple quantified pattern
                @"[a-zA-Z]+@\w+\.\w+",         // Email-like pattern
                @"\p{L}+\x{41}\x42",           // Unicode properties with hex escapes
                @"(?:abc|def)+",               // Non-capturing group with alternation
                @"\A[[:alpha:]]+\Z",           // POSIX character classes with anchors
                @"(\w+)\s+\1",                 // Back-reference (ambiguous!)
                @"\x{1F600}\x{1F601}",         // Unicode emoji
                @"a{2,5}?b*c+?"                // Complex quantifiers (ambiguous!)
            };
            
            var controller = new PatternParser(ParserType.Regex);
            
            foreach (var pattern in testPatterns)
            {
                Console.WriteLine($"Testing pattern: {pattern}");
                DemonstrateZeroCopyParsing(controller, pattern);
                Console.WriteLine();
            }
            
            // Demonstrate memory efficiency
            DemonstrateMemoryEfficiency();
            
            // Demonstrate UTF-8 processing
            DemonstrateUTF8Processing();
        }
        
        private static void DemonstrateZeroCopyParsing(PatternParser controller, string pattern)
        {
            try
            {
                // Convert to UTF-8 once, then process without further allocations
                var utf8Bytes = Encoding.UTF8.GetBytes(pattern);
                Console.WriteLine($"  UTF-8 bytes: {utf8Bytes.Length} bytes");
                
                // Zero-copy parsing
                bool success = controller.ParsePattern(utf8Bytes.AsSpan(), $"pattern_{pattern.GetHashCode():X}");
                
                var results = controller.GetResults();
                
                Console.WriteLine($"  Phase 1 tokens: {results.Phase1TokenCount}");
                Console.WriteLine($"  Ambiguous tokens: {results.AmbiguousTokenCount}");
                Console.WriteLine($"  Ambiguity ratio: {results.AmbiguityRatio:P1}");
                Console.WriteLine($"  Memory used: {results.MemoryUsed} bytes");
                Console.WriteLine($"  Parsing: {(success ? "SUCCESS" : "FAILED")}");
                
                if (results.AmbiguousTokenCount > 0)
                {
                    Console.WriteLine("  ⚠️  Pattern contains ambiguous constructs - two-phase parsing resolved them");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Error: {ex.Message}");
            }
        }
        
        private static void DemonstrateMemoryEfficiency()
        {
            Console.WriteLine("=== Memory Efficiency Demonstration ===");
            
            // Create a large pattern to show memory savings
            var largePattern = string.Join("|", Enumerable.Repeat(@"\w+\d{2,4}", 100));
            
            Console.WriteLine($"Large pattern with {largePattern.Length} characters");
            
            // Traditional approach (UTF-16)
            var traditionalMemory = largePattern.Length * 2; // UTF-16 uses 2 bytes per char
            Console.WriteLine($"Traditional UTF-16 memory: {traditionalMemory} bytes");
            
            // vNext approach (UTF-8)
            var utf8Bytes = Encoding.UTF8.GetBytes(largePattern);
            Console.WriteLine($"Modern UTF-8 memory: {utf8Bytes.Length} bytes");
            Console.WriteLine($"Memory savings: {((double)(traditionalMemory - utf8Bytes.Length) / traditionalMemory):P1}");
            
            // Zero-copy slicing demonstration
            var view = new ZeroCopyStringView(utf8Bytes);
            var slice1 = view.Slice(0, 10);
            var slice2 = view.Slice(10, 10);
            
            Console.WriteLine($"Zero-copy slice 1: {slice1} (no allocation)");
            Console.WriteLine($"Zero-copy slice 2: {slice2} (no allocation)");
        }
        
        private static void DemonstrateUTF8Processing()
        {
            Console.WriteLine("=== UTF-8 Processing Demonstration ===");
            
            // Test patterns with various Unicode constructs
            var unicodePatterns = new[]
            {
                "café",                        // Simple Latin-1 supplement
                "🦄🌈",                        // Emoji (4-byte UTF-8)
                "Ĥéłłø",                       // Various diacritics
                "中文测试",                      // Chinese characters
                "Проверка",                    // Cyrillic
                "εδοκιμασία"                   // Greek
            };
            
            foreach (var pattern in unicodePatterns)
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(pattern);
                var view = new ZeroCopyStringView(utf8Bytes);
                
                Console.WriteLine($"Pattern: {pattern}");
                Console.WriteLine($"  UTF-16 size: {pattern.Length * 2} bytes");
                Console.WriteLine($"  UTF-8 size: {utf8Bytes.Length} bytes");
                Console.WriteLine($"  Zero-copy view length: {view.Length}");
                
                // Demonstrate UTF-8 codepoint processing
                int position = 0;
                int codepointCount = 0;
                
                while (position < view.Length)
                {
                    var (codepoint, bytesConsumed) = UTF8Utils.GetNextCodepoint(view.AsSpan(), position);
                    if (bytesConsumed == 0) break;
                    
                    codepointCount++;
                    position += bytesConsumed;
                }
                
                Console.WriteLine($"  Unicode codepoints: {codepointCount}");
                Console.WriteLine();
            }
        }
    }
}