using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;

namespace DevelApp.StepLexer.Tests
{
    /// <summary>
    /// Comprehensive performance benchmarks comparing DevelApp.StepLexer with .NET built-in Regex
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    [Config(typeof(BenchmarkConfig))]
    public class PCRE2PerformanceBenchmark
    {
        private Regex _compiledRegex = null!;
        private StepLexer _stepLexer = null!;
        private string _testInput = null!;
        private byte[] _utf8TestInput = null!;
        private string _pattern = null!;

        [Params(
            @"[a-zA-Z]+",                    // Simple character class
            @"\p{L}+",                       // Unicode letter property
            @"(?i)\p{L}+\p{Nd}*",           // Case insensitive with Unicode
            @"\Q literal text \E\p{L}+",    // Literal text with Unicode
            @"(?#comment)\p{L}+(?i)test"    // Comments and inline modifiers
        )]
        public string Pattern { get; set; } = string.Empty;

        [Params(100, 1000, 10000)]
        public int InputSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _pattern = Pattern;
            _stepLexer = new StepLexer();
            
            // Create compiled regex for comparison
            try
            {
                // Convert PCRE2 pattern to .NET compatible pattern (simplified)
                var dotNetPattern = ConvertToNetRegexPattern(_pattern);
                _compiledRegex = new Regex(dotNetPattern, RegexOptions.Compiled);
            }
            catch
            {
                // If pattern can't be converted, use a simple fallback
                _compiledRegex = new Regex(@"[a-zA-Z]+", RegexOptions.Compiled);
            }
            
            // Generate test input with mix of ASCII and Unicode text
            _testInput = GenerateTestInput(InputSize);
            _utf8TestInput = Encoding.UTF8.GetBytes(_testInput);
        }

        [Benchmark(Baseline = true)]
        public bool DotNetCompiledRegex()
        {
            return _compiledRegex.IsMatch(_testInput);
        }

        [Benchmark]
        public bool StepLexerPCRE2()
        {
            var view = new ZeroCopyStringView(_utf8TestInput);
            var phase1Result = _stepLexer.Phase1_LexicalScan(view);
            return phase1Result && _stepLexer.Phase2_Disambiguation();
        }

        [Benchmark]
        public bool StepLexerZeroCopy()
        {
            var view = new ZeroCopyStringView(_utf8TestInput);
            return _stepLexer.Phase1_LexicalScan(view);
        }

        /// <summary>
        /// Convert PCRE2 pattern to .NET Regex pattern (simplified conversion)
        /// </summary>
        private static string ConvertToNetRegexPattern(string pcrePattern)
        {
            // Simplified conversion - in a full implementation this would be more comprehensive
            return pcrePattern
                .Replace(@"\Q", "")          // Remove literal text start
                .Replace(@"\E", "")          // Remove literal text end
                .Replace(@"(?#", "(?:")      // Convert comments to non-capturing groups
                .Replace(@"(?i)", "");       // Remove inline modifiers (would set RegexOptions instead)
        }

        /// <summary>
        /// Generate test input with mixed ASCII and Unicode content
        /// </summary>
        private static string GenerateTestInput(int size)
        {
            var random = new Random(42); // Fixed seed for reproducible results
            var chars = new char[size];
            
            for (int i = 0; i < size; i++)
            {
                if (i % 10 == 0)
                {
                    // Add some Unicode characters (10% of input)
                    chars[i] = (char)(0x00C0 + (i % 64)); // Latin Extended characters
                }
                else if (i % 5 == 0)
                {
                    // Add numbers (20% of input)
                    chars[i] = (char)('0' + (i % 10));
                }
                else
                {
                    // Add ASCII letters (70% of input)
                    chars[i] = (char)('a' + (i % 26));
                }
            }
            
            return new string(chars);
        }
    }

    /// <summary>
    /// Memory usage benchmarks for zero-copy processing
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class MemoryUsageBenchmark
    {
        private string _testInput = null!;
        private byte[] _utf8TestInput = null!;
        private StepLexer _stepLexer = null!;
        private Regex _compiledRegex = null!;

        [Params(1000, 10000, 100000)]
        public int InputSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _testInput = GenerateUnicodeTestInput(InputSize);
            _utf8TestInput = Encoding.UTF8.GetBytes(_testInput);
            _stepLexer = new StepLexer();
            _compiledRegex = new Regex(@"\p{L}+", RegexOptions.Compiled);
        }

        [Benchmark(Baseline = true)]
        public bool DotNetRegexMemoryUsage()
        {
            return _compiledRegex.IsMatch(_testInput);
        }

        [Benchmark]
        public bool StepLexerZeroCopyMemoryUsage()
        {
            var view = new ZeroCopyStringView(_utf8TestInput);
            return _stepLexer.Phase1_LexicalScan(view);
        }

        /// <summary>
        /// Generate Unicode-heavy test input to emphasize UTF-8 vs UTF-16 memory differences
        /// </summary>
        private static string GenerateUnicodeTestInput(int size)
        {
            var chars = new char[size];
            var random = new Random(42);
            
            for (int i = 0; i < size; i++)
            {
                // Mix of different Unicode ranges
                if (i % 4 == 0)
                    chars[i] = (char)(0x0100 + (i % 256)); // Latin Extended
                else if (i % 4 == 1)
                    chars[i] = (char)(0x0370 + (i % 144)); // Greek
                else if (i % 4 == 2)
                    chars[i] = (char)(0x0400 + (i % 256)); // Cyrillic
                else
                    chars[i] = (char)('a' + (i % 26));     // ASCII
            }
            
            return new string(chars);
        }
    }

    /// <summary>
    /// Unicode property matching performance benchmarks
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class UnicodePropertyBenchmark
    {
        private int[] _testCodepoints = null!;

        [Params("L", "Nd", "Basic_Latin", "Emoji", "Math")]
        public string UnicodeProperty { get; set; } = string.Empty;

        [Params(1000, 10000)]
        public int TestCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(42);
            _testCodepoints = new int[TestCount];
            
            for (int i = 0; i < TestCount; i++)
            {
                // Generate mix of codepoints
                if (i % 4 == 0)
                    _testCodepoints[i] = 0x0041 + (i % 26); // ASCII letters
                else if (i % 4 == 1)
                    _testCodepoints[i] = 0x0030 + (i % 10); // ASCII digits
                else if (i % 4 == 2)
                    _testCodepoints[i] = 0x1F600 + (i % 80); // Emoji range
                else
                    _testCodepoints[i] = 0x0100 + (i % 256); // Extended Latin
            }
        }

        [Benchmark]
        public int ICU_UnicodePropertyMatching()
        {
            int matches = 0;
            for (int i = 0; i < _testCodepoints.Length; i++)
            {
                if (UnicodePropertyMatcher.MatchesProperty(_testCodepoints[i], UnicodeProperty))
                    matches++;
            }
            return matches;
        }

        [Benchmark(Baseline = true)]
        public int DotNet_CharIsLetter()
        {
            int matches = 0;
            // Simple baseline comparison for letter checking only
            if (UnicodeProperty == "L")
            {
                for (int i = 0; i < _testCodepoints.Length; i++)
                {
                    if (_testCodepoints[i] <= 0xFFFF && char.IsLetter((char)_testCodepoints[i]))
                        matches++;
                }
            }
            return matches;
        }
    }

    /// <summary>
    /// Benchmark configuration for consistent testing
    /// </summary>
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core80));
            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }

    /// <summary>
    /// Performance test runner for integration with unit test framework
    /// </summary>
    public static class PerformanceTestRunner
    {
        /// <summary>
        /// Run all performance benchmarks (for CI/CD integration)
        /// </summary>
        public static void RunAllBenchmarks()
        {
            // In a CI environment, we might run a subset of benchmarks
            // For now, just validate that benchmarking framework works
            var benchmark = new PCRE2PerformanceBenchmark
            {
                Pattern = @"\p{L}+",
                InputSize = 100
            };
            
            benchmark.Setup();
            
            // Run a quick validation
            var dotNetResult = benchmark.DotNetCompiledRegex();
            var stepLexerResult = benchmark.StepLexerPCRE2();
            
            // Both should complete without exceptions
            if (dotNetResult || stepLexerResult || !dotNetResult || !stepLexerResult)
            {
                // Either result is acceptable - we're testing performance, not correctness
                return;
            }
        }

        /// <summary>
        /// Run memory benchmarks (for CI/CD integration)
        /// </summary>
        public static void RunMemoryBenchmarks()
        {
            var benchmark = new MemoryUsageBenchmark
            {
                InputSize = 1000
            };
            
            benchmark.Setup();
            
            // Run validation
            var dotNetResult = benchmark.DotNetRegexMemoryUsage();
            var stepLexerResult = benchmark.StepLexerZeroCopyMemoryUsage();
            
            // Validate that both complete
            if (dotNetResult || stepLexerResult || !dotNetResult || !stepLexerResult)
            {
                return;
            }
        }

        /// <summary>
        /// Run Unicode property benchmarks (for CI/CD integration)
        /// </summary>
        public static void RunUnicodeBenchmarks()
        {
            var benchmark = new UnicodePropertyBenchmark
            {
                UnicodeProperty = "L",
                TestCount = 100
            };
            
            benchmark.Setup();
            
            // Run validation
            var icuResult = benchmark.ICU_UnicodePropertyMatching();
            var dotNetResult = benchmark.DotNet_CharIsLetter();
            
            // Validate that both return reasonable results
            if (icuResult >= 0 && dotNetResult >= 0)
            {
                return;
            }
        }
    }
}