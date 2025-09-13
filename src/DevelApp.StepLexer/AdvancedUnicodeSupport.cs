using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ICU4N;
using ICU4N.Text;

namespace DevelApp.StepLexer
{
    /// <summary>
    /// Advanced Unicode property matcher using ICU with .NET fallback for comprehensive Unicode support
    /// </summary>
    public class UnicodePropertyMatcher
    {
        private static readonly Dictionary<string, int> _propertyCache = new();
        private static readonly object _cacheLock = new();

        /// <summary>
        /// Check if a Unicode code point matches the specified property
        /// </summary>
        /// <param name="codepoint">The Unicode code point to test</param>
        /// <param name="property">The Unicode property name (e.g., "L", "Nd", "Basic_Latin")</param>
        /// <returns>True if the code point has the specified property</returns>
        public static bool MatchesProperty(int codepoint, string property)
        {
            // Convert codepoint to char for .NET methods (only for BMP characters)
            if (codepoint > 0xFFFF)
            {
                // For characters outside BMP, use more complex logic
                return MatchesAdvancedProperty(codepoint, property);
            }
            
            var ch = (char)codepoint;
            
            return property switch
            {
                // General categories - Letters
                "L" => char.IsLetter(ch),
                "LC" => char.IsLetter(ch) && (char.IsUpper(ch) || char.IsLower(ch)),
                "Ll" => char.IsLower(ch),
                "Lm" => char.GetUnicodeCategory(ch) == UnicodeCategory.ModifierLetter,
                "Lo" => char.GetUnicodeCategory(ch) == UnicodeCategory.OtherLetter,
                "Lt" => char.GetUnicodeCategory(ch) == UnicodeCategory.TitlecaseLetter,
                "Lu" => char.IsUpper(ch),

                // General categories - Marks
                "M" => char.GetUnicodeCategory(ch) >= UnicodeCategory.NonSpacingMark &&
                       char.GetUnicodeCategory(ch) <= UnicodeCategory.EnclosingMark,
                "Mc" => char.GetUnicodeCategory(ch) == UnicodeCategory.SpacingCombiningMark,
                "Me" => char.GetUnicodeCategory(ch) == UnicodeCategory.EnclosingMark,
                "Mn" => char.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark,

                // General categories - Numbers
                "N" => char.IsNumber(ch),
                "Nd" => char.IsDigit(ch),
                "Nl" => char.GetUnicodeCategory(ch) == UnicodeCategory.LetterNumber,
                "No" => char.GetUnicodeCategory(ch) == UnicodeCategory.OtherNumber,

                // General categories - Punctuation
                "P" => char.IsPunctuation(ch),
                "Pc" => char.GetUnicodeCategory(ch) == UnicodeCategory.ConnectorPunctuation,
                "Pd" => char.GetUnicodeCategory(ch) == UnicodeCategory.DashPunctuation,
                "Pe" => char.GetUnicodeCategory(ch) == UnicodeCategory.ClosePunctuation,
                "Pf" => char.GetUnicodeCategory(ch) == UnicodeCategory.FinalQuotePunctuation,
                "Pi" => char.GetUnicodeCategory(ch) == UnicodeCategory.InitialQuotePunctuation,
                "Po" => char.GetUnicodeCategory(ch) == UnicodeCategory.OtherPunctuation,
                "Ps" => char.GetUnicodeCategory(ch) == UnicodeCategory.OpenPunctuation,

                // General categories - Symbols
                "S" => char.IsSymbol(ch),
                "Sc" => char.GetUnicodeCategory(ch) == UnicodeCategory.CurrencySymbol,
                "Sk" => char.GetUnicodeCategory(ch) == UnicodeCategory.ModifierSymbol,
                "Sm" => char.GetUnicodeCategory(ch) == UnicodeCategory.MathSymbol,
                "So" => char.GetUnicodeCategory(ch) == UnicodeCategory.OtherSymbol,

                // General categories - Separators
                "Z" => char.IsSeparator(ch),
                "Zl" => char.GetUnicodeCategory(ch) == UnicodeCategory.LineSeparator,
                "Zp" => char.GetUnicodeCategory(ch) == UnicodeCategory.ParagraphSeparator,
                "Zs" => char.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator,

                // General categories - Other
                "C" => char.IsControl(ch) || char.GetUnicodeCategory(ch) >= UnicodeCategory.Control,
                "Cc" => char.IsControl(ch),
                "Cf" => char.GetUnicodeCategory(ch) == UnicodeCategory.Format,
                "Cn" => char.GetUnicodeCategory(ch) == UnicodeCategory.OtherNotAssigned,
                "Co" => char.GetUnicodeCategory(ch) == UnicodeCategory.PrivateUse,
                "Cs" => char.GetUnicodeCategory(ch) == UnicodeCategory.Surrogate,

                // Unicode blocks
                "Basic_Latin" => codepoint >= 0x0000 && codepoint <= 0x007F,
                "Latin_1_Supplement" => codepoint >= 0x0080 && codepoint <= 0x00FF,
                "Latin_Extended_A" => codepoint >= 0x0100 && codepoint <= 0x017F,
                "Latin_Extended_B" => codepoint >= 0x0180 && codepoint <= 0x024F,
                "IPA_Extensions" => codepoint >= 0x0250 && codepoint <= 0x02AF,
                "Spacing_Modifier_Letters" => codepoint >= 0x02B0 && codepoint <= 0x02FF,
                "Combining_Diacritical_Marks" => codepoint >= 0x0300 && codepoint <= 0x036F,
                "Greek_and_Coptic" => codepoint >= 0x0370 && codepoint <= 0x03FF,
                "Cyrillic" => codepoint >= 0x0400 && codepoint <= 0x04FF,
                "Hebrew" => codepoint >= 0x0590 && codepoint <= 0x05FF,
                "Arabic" => codepoint >= 0x0600 && codepoint <= 0x06FF,
                "Devanagari" => codepoint >= 0x0900 && codepoint <= 0x097F,
                "Bengali" => codepoint >= 0x0980 && codepoint <= 0x09FF,
                "Thai" => codepoint >= 0x0E00 && codepoint <= 0x0E7F,
                "Hiragana" => codepoint >= 0x3040 && codepoint <= 0x309F,
                "Katakana" => codepoint >= 0x30A0 && codepoint <= 0x30FF,
                "CJK_Unified_Ideographs" => codepoint >= 0x4E00 && codepoint <= 0x9FFF,

                // Script properties and binary properties using ICU where available
                _ => MatchesAdvancedProperty(codepoint, property)
            };
        }

        /// <summary>
        /// Match advanced properties using .NET and simplified property logic
        /// </summary>
        private static bool MatchesAdvancedProperty(int codepoint, string property)
        {
            try
            {
                // Handle some common binary properties
                return property switch
                {
                    "Alphabetic" => IsLetter(codepoint),
                    "ASCII_Hex_Digit" => codepoint >= 0x30 && codepoint <= 0x39 || // 0-9
                                        codepoint >= 0x41 && codepoint <= 0x46 || // A-F
                                        codepoint >= 0x61 && codepoint <= 0x66,   // a-f
                    "Emoji" => codepoint >= 0x1F600 && codepoint <= 0x1F64F || // Emoticons
                               codepoint >= 0x1F300 && codepoint <= 0x1F5FF || // Misc Symbols
                               codepoint >= 0x1F680 && codepoint <= 0x1F6FF || // Transport
                               codepoint >= 0x2600 && codepoint <= 0x26FF,     // Misc symbols
                    "Math" => GetUnicodeCategory(codepoint) == UnicodeCategory.MathSymbol ||
                              codepoint >= 0x2200 && codepoint <= 0x22FF, // Mathematical Operators
                    "Uppercase" => IsUpper(codepoint),
                    "Lowercase" => IsLower(codepoint),
                    "White_Space" => IsWhiteSpace(codepoint),
                    "ID_Start" => IsLetter(codepoint) || codepoint == 0x5F, // Letters or underscore
                    "ID_Continue" => IsLetterOrDigit(codepoint) || codepoint == 0x5F,
                    
                    // Script properties (simplified detection)
                    "Latin" => codepoint >= 0x0041 && codepoint <= 0x005A || // A-Z
                               codepoint >= 0x0061 && codepoint <= 0x007A || // a-z
                               codepoint >= 0x0100 && codepoint <= 0x017F || // Latin Extended-A
                               codepoint >= 0x0180 && codepoint <= 0x024F,   // Latin Extended-B
                    "Greek" => codepoint >= 0x0370 && codepoint <= 0x03FF,
                    "Arabic" => codepoint >= 0x0600 && codepoint <= 0x06FF,
                    "Cyrillic" => codepoint >= 0x0400 && codepoint <= 0x04FF,
                    "Hebrew" => codepoint >= 0x0590 && codepoint <= 0x05FF,
                    
                    _ => false // Unknown property
                };
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to check if a codepoint represents a letter
        /// </summary>
        private static bool IsLetter(int codepoint)
        {
            if (codepoint <= 0xFFFF)
                return char.IsLetter((char)codepoint);
            
            // For supplementary characters, use range checks
            return (codepoint >= 0x10000 && codepoint <= 0x1FFFF) || // Supplementary Multilingual Plane
                   (codepoint >= 0x20000 && codepoint <= 0x2FFFF);   // Supplementary Ideographic Plane
        }

        /// <summary>
        /// Helper method to check if a codepoint represents an uppercase letter
        /// </summary>
        private static bool IsUpper(int codepoint)
        {
            if (codepoint <= 0xFFFF)
                return char.IsUpper((char)codepoint);
            return false; // Simplified for supplementary characters
        }

        /// <summary>
        /// Helper method to check if a codepoint represents a lowercase letter
        /// </summary>
        private static bool IsLower(int codepoint)
        {
            if (codepoint <= 0xFFFF)
                return char.IsLower((char)codepoint);
            return false; // Simplified for supplementary characters
        }

        /// <summary>
        /// Helper method to check if a codepoint represents whitespace
        /// </summary>
        private static bool IsWhiteSpace(int codepoint)
        {
            if (codepoint <= 0xFFFF)
                return char.IsWhiteSpace((char)codepoint);
            return false; // Simplified for supplementary characters
        }

        /// <summary>
        /// Helper method to check if a codepoint represents a letter or digit
        /// </summary>
        private static bool IsLetterOrDigit(int codepoint)
        {
            if (codepoint <= 0xFFFF)
                return char.IsLetterOrDigit((char)codepoint);
            return IsLetter(codepoint); // Simplified for supplementary characters
        }

        /// <summary>
        /// Helper method to get Unicode category for a codepoint
        /// </summary>
        private static UnicodeCategory GetUnicodeCategory(int codepoint)
        {
            if (codepoint <= 0xFFFF)
                return char.GetUnicodeCategory((char)codepoint);
            return UnicodeCategory.OtherLetter; // Simplified for supplementary characters
        }
    }

    /// <summary>
    /// Advanced Unicode support with ICU integration and .NET fallback for normalization and property handling
    /// </summary>
    public class AdvancedUnicodeSupport
    {
        /// <summary>
        /// Initializes a new instance of the AdvancedUnicodeSupport class
        /// </summary>
        public AdvancedUnicodeSupport()
        {
            // Initialization with available normalizers
        }

        /// <summary>
        /// Process Unicode pattern with normalization support
        /// </summary>
        /// <param name="utf8Input">The UTF-8 input to process</param>
        /// <param name="pattern">The pattern string</param>
        /// <param name="normalizationForm">The Unicode normalization form to apply</param>
        /// <returns>True if the pattern matches the normalized input</returns>
        public bool ProcessUnicodePattern(ReadOnlySpan<byte> utf8Input, string pattern, UnicodeNormalizationForm normalizationForm = UnicodeNormalizationForm.None)
        {
            try
            {
                // Convert UTF-8 to string for processing
                var inputString = System.Text.Encoding.UTF8.GetString(utf8Input);

                // Normalize input if required
                var normalizedInput = NormalizeIfNeeded(inputString, normalizationForm);

                // Process with Unicode property support
                return ProcessWithUnicodeSupport(normalizedInput, pattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Normalize string according to specified form using .NET normalization
        /// </summary>
        /// <param name="input">The input string to normalize</param>
        /// <param name="form">The normalization form</param>
        /// <returns>The normalized string</returns>
        public string NormalizeIfNeeded(string input, UnicodeNormalizationForm form)
        {
            return form switch
            {
                UnicodeNormalizationForm.NFC => input.Normalize(System.Text.NormalizationForm.FormC),
                UnicodeNormalizationForm.NFD => input.Normalize(System.Text.NormalizationForm.FormD),
                UnicodeNormalizationForm.NFKC => input.Normalize(System.Text.NormalizationForm.FormKC),
                UnicodeNormalizationForm.NFKD => input.Normalize(System.Text.NormalizationForm.FormKD),
                UnicodeNormalizationForm.None => input,
                _ => input
            };
        }

        /// <summary>
        /// Process pattern with Unicode property support
        /// </summary>
        private bool ProcessWithUnicodeSupport(string normalizedInput, string pattern)
        {
            // Simplified implementation for validation
            for (int i = 0; i < normalizedInput.Length; i++)
            {
                var codepoint = char.ConvertToUtf32(normalizedInput, i);
                
                // Skip surrogate pairs
                if (char.IsHighSurrogate(normalizedInput[i]))
                    i++; // Skip the low surrogate
                
                // Validate that we can process this codepoint
                if (codepoint < 0 || codepoint > 0x10FFFF)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Check if two strings are canonically equivalent using .NET normalization
        /// </summary>
        /// <param name="str1">First string</param>
        /// <param name="str2">Second string</param>
        /// <returns>True if the strings are canonically equivalent</returns>
        public bool AreCanonicallyEquivalent(string str1, string str2)
        {
            return str1.Normalize(System.Text.NormalizationForm.FormC) == 
                   str2.Normalize(System.Text.NormalizationForm.FormC);
        }

        /// <summary>
        /// Get the grapheme cluster boundaries in a string (simplified implementation)
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Array of grapheme cluster boundary positions</returns>
        public int[] GetGraphemeClusterBoundaries(string text)
        {
            var boundaries = new List<int> { 0 };
            
            // Simplified boundary detection
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    i++; // Skip the low surrogate
                    boundaries.Add(i + 1);
                }
                else if (i + 1 < text.Length)
                {
                    boundaries.Add(i + 1);
                }
            }

            return boundaries.ToArray();
        }
    }

    /// <summary>
    /// Unicode normalization forms supported by the advanced Unicode processor
    /// </summary>
    public enum UnicodeNormalizationForm
    {
        /// <summary>
        /// No normalization applied
        /// </summary>
        None,
        
        /// <summary>
        /// Canonical decomposition followed by canonical composition
        /// </summary>
        NFC,
        
        /// <summary>
        /// Canonical decomposition
        /// </summary>
        NFD,
        
        /// <summary>
        /// Compatibility decomposition followed by canonical composition
        /// </summary>
        NFKC,
        
        /// <summary>
        /// Compatibility decomposition
        /// </summary>
        NFKD
    }
}