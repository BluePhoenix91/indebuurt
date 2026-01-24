using System.Text.RegularExpressions;

namespace Pipeline.Cli.Services;

/// <summary>
/// Converts Dutch text from UPPERCASE to Title Case with proper handling of:
/// - Dutch articles and prepositions (de, het, van, etc.)
/// - Apostrophe prefixes ('t Eilandje)
/// - Hyphenated words (Sint-Amandsberg)
/// - Parenthetical content (Atheneum (Stationswijk))
/// </summary>
public partial class DutchTitleCaseConverter
{
    // Dutch words that should remain lowercase (except at start of name)
    private static readonly HashSet<string> LowercaseWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "het", "een", "van", "in", "op", "aan", "te", "bij", "voor", "met", "tot", "over"
    };

    /// <summary>
    /// Converts an UPPERCASE string to Title Case using Dutch language rules.
    /// </summary>
    /// <param name="input">The input string, typically in UPPERCASE</param>
    /// <returns>The string in Title Case</returns>
    public string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // If already mixed case (not all uppercase), return as-is
        if (!IsAllUpperCase(input))
            return input;

        // Convert to lowercase first
        var lower = input.ToLowerInvariant();

        // Handle special prefix "'t " at the start
        if (lower.StartsWith("'t "))
        {
            return "'t " + ToTitleCaseInternal(lower[3..], isStart: true);
        }

        return ToTitleCaseInternal(lower, isStart: true);
    }

    private static bool IsAllUpperCase(string input)
    {
        return input.Any(char.IsLetter) &&
               input.Where(char.IsLetter).All(char.IsUpper);
    }

    private string ToTitleCaseInternal(string input, bool isStart)
    {
        // Split by spaces and parentheses, keeping delimiters
        var parts = WordSplitRegex().Split(input);
        var result = new List<string>();
        var wordIndex = 0;

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            // Keep delimiters as-is
            if (part == "(" || part == ")" || string.IsNullOrWhiteSpace(part))
            {
                result.Add(part);
                // Reset word index inside parentheses so first word gets capitalized
                if (part == "(")
                    wordIndex = 0;
                continue;
            }

            var word = part;

            // Handle hyphenated words: each part gets Title Case
            if (word.Contains('-'))
            {
                var hyphenParts = word.Split('-');
                word = string.Join("-", hyphenParts.Select((p, i) => CapitalizeWord(p, i == 0 && wordIndex == 0)));
            }
            else
            {
                word = CapitalizeWord(word, wordIndex == 0);
            }

            result.Add(word);
            wordIndex++;
        }

        return string.Concat(result);
    }

    private static string CapitalizeWord(string word, bool isFirstWord)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        // Handle "'t" prefix within word (e.g., "'t" standalone or "'tfort")
        if (word.StartsWith("'t"))
        {
            if (word.Length == 2)
                return "'t";
            return "'t" + char.ToUpperInvariant(word[2]) + word[3..];
        }

        // Keep Dutch articles/prepositions lowercase unless first word
        if (!isFirstWord && LowercaseWords.Contains(word))
        {
            return word;
        }

        // Standard title case: capitalize first letter
        return char.ToUpperInvariant(word[0]) + word[1..];
    }

    [GeneratedRegex(@"(\s+|\(|\))")]
    private static partial Regex WordSplitRegex();
}
