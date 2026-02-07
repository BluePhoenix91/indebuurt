using System.Text;
using System.Text.RegularExpressions;

namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Generates URL-safe slugs for neighborhood and sector IDs.
/// Reimplements the PostgreSQL slugify() function from the legacy schema.
/// </summary>
public interface ISlugGenerator
{
    /// <summary>
    /// Convert text to a URL-safe slug.
    /// Transliterates accented chars, removes special chars, replaces spaces with hyphens.
    /// </summary>
    string Slugify(string text);

    /// <summary>
    /// Generate neighborhood slugs from metadata, handling duplicates by appending NIS code.
    /// Returns a dictionary mapping NIS code to slug.
    /// </summary>
    Dictionary<string, string> GenerateNeighborhoodSlugs(IEnumerable<NeighborhoodMetadata> neighborhoods);

    /// <summary>
    /// Generate a sector slug from city, sector name, and sector code.
    /// Handles duplicates by appending the sector code as suffix.
    /// </summary>
    Dictionary<string, string> GenerateSectorSlugs(IEnumerable<(string CdSector, string City, string Name)> sectors);
}

public partial class SlugGenerator : ISlugGenerator
{
    // Transliteration map matching the legacy PostgreSQL slugify() function exactly
    private const string AccentedChars =
        "àáâãäåæçèéêëìíîïñòóôõöøùúûüýÿÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖØÙÚÛÜÝ";
    private const string TransliteratedChars =
        "aaaaaaaceeeeiiiinooooooouuuuyyAAAAAAACEEEEIIIINOOOOOOUUUUY";

    private static readonly Dictionary<char, char> TransliterationMap = BuildTransliterationMap();

    public string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);

        // Transliterate accented characters
        foreach (var c in text)
        {
            sb.Append(TransliterationMap.GetValueOrDefault(c, c));
        }

        var result = sb.ToString();

        // Remove special characters (keep alphanumeric, spaces, hyphens)
        result = SpecialCharsRegex().Replace(result, "");

        // Replace spaces with hyphens
        result = SpacesRegex().Replace(result, "-");

        // Collapse multiple hyphens
        result = MultipleHyphensRegex().Replace(result, "-");

        // Trim leading/trailing hyphens and lowercase
        return result.Trim('-').ToLowerInvariant();
    }

    public Dictionary<string, string> GenerateNeighborhoodSlugs(IEnumerable<NeighborhoodMetadata> neighborhoods)
    {
        var items = neighborhoods.ToList();

        // Generate base slugs
        var slugEntries = items.Select(n => new
        {
            n.NisCode,
            BaseSlug = Slugify(n.City) + "-" + Slugify(n.Name)
        }).ToList();

        // Detect duplicates
        var slugCounts = slugEntries
            .GroupBy(e => e.BaseSlug)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new Dictionary<string, string>();
        foreach (var entry in slugEntries)
        {
            var slug = slugCounts[entry.BaseSlug] > 1
                ? entry.BaseSlug + "-" + entry.NisCode.ToLowerInvariant()
                : entry.BaseSlug;
            result[entry.NisCode] = slug;
        }

        return result;
    }

    public Dictionary<string, string> GenerateSectorSlugs(IEnumerable<(string CdSector, string City, string Name)> sectors)
    {
        var items = sectors.ToList();

        var slugEntries = items.Select(s => new
        {
            s.CdSector,
            BaseSlug = Slugify(s.City) + "-" + Slugify(s.Name)
        }).ToList();

        var slugCounts = slugEntries
            .GroupBy(e => e.BaseSlug)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new Dictionary<string, string>();
        foreach (var entry in slugEntries)
        {
            var slug = slugCounts[entry.BaseSlug] > 1
                ? entry.BaseSlug + "-" + entry.CdSector.ToLowerInvariant()
                : entry.BaseSlug;
            result[entry.CdSector] = slug;
        }

        return result;
    }

    private static Dictionary<char, char> BuildTransliterationMap()
    {
        var map = new Dictionary<char, char>();
        for (var i = 0; i < AccentedChars.Length; i++)
        {
            map[AccentedChars[i]] = TransliteratedChars[i];
        }
        return map;
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\s-]")]
    private static partial Regex SpecialCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}
