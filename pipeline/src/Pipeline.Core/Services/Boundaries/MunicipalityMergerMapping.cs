namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Maps old municipality NIS codes to the new merged codes after the 2025 Belgian municipal mergers.
/// Source: Statbel REFNIS-NUTS 2025 (from database/migrations/20260110_008_add-statbel-municipality-nis.sql).
/// </summary>
public static class MunicipalityMergerMapping
{
    /// <summary>
    /// Old 5-char municipality NIS code → new merged 5-char municipality NIS code.
    /// Only contains entries where the code changed; unchanged municipalities are not listed.
    /// </summary>
    private static readonly Dictionary<string, string> Mergers = new()
    {
        // East Flanders
        ["44040"] = "44088", // Melle → Merelbeke-Melle
        ["44043"] = "44088", // Merelbeke → Merelbeke-Melle
        ["44034"] = "44087", // Lochristi → Lochristi (absorbed Wachtebeke)
        ["44073"] = "44087", // Wachtebeke → Lochristi
        ["44012"] = "44086", // De Pinte → Nazareth-De Pinte
        ["44048"] = "44086", // Nazareth → Nazareth-De Pinte
        ["46003"] = "46030", // Beveren → Beveren-Kruibeke-Zwijndrecht
        ["46013"] = "46030", // Kruibeke → Beveren-Kruibeke-Zwijndrecht
        ["11056"] = "46030", // Zwijndrecht → Beveren-Kruibeke-Zwijndrecht
        ["46014"] = "46029", // Lokeren → Lokeren (absorbed Moerbeke)
        ["44045"] = "46029", // Moerbeke → Lokeren

        // Limburg
        ["71022"] = "71072", // Hasselt → Hasselt (absorbed Kortessem)
        ["73040"] = "71072", // Kortessem → Hasselt
        ["73006"] = "73110", // Bilzen → Bilzen-Hoeselt
        ["73032"] = "73110", // Hoeselt → Bilzen-Hoeselt
        ["73083"] = "73111", // Tongeren → Tongeren-Borgloon
        ["73009"] = "73111", // Borgloon → Tongeren-Borgloon
        ["71057"] = "71071", // Tessenderlo → Tessenderlo-Ham
        ["71069"] = "71071", // Ham → Tessenderlo-Ham

        // Flemish Brabant
        ["23023"] = "23106", // Galmaarden → Pajottegem
        ["23024"] = "23106", // Gooik → Pajottegem
        ["23032"] = "23106", // Herne → Pajottegem

        // Antwerp
        ["11007"] = "11002", // Borsbeek → Antwerpen

        // West Flanders
        ["37015"] = "37022", // Tielt → Tielt (absorbed Meulebeke)
        ["37007"] = "37022", // Meulebeke → Tielt
        ["37018"] = "37021", // Wingene → Wingene (absorbed Ruiselede)
        ["37012"] = "37021", // Ruiselede → Wingene
    };

    /// <summary>
    /// Get the Statbel-compatible municipality NIS code for a given neighborhood NIS code.
    /// Returns the merged code if the municipality was part of a 2025 merger,
    /// or the first 5 characters of the NIS code (the municipality portion) if unchanged.
    /// </summary>
    public static string GetStatbelMunicipalityNis(string nisCode)
    {
        var municipalityNis = nisCode[..5];
        return Mergers.GetValueOrDefault(municipalityNis, municipalityNis);
    }

    /// <summary>
    /// Get all merger mappings (for SQL UPDATE statements).
    /// Returns grouped entries: new NIS code → list of old NIS codes.
    /// </summary>
    public static Dictionary<string, List<string>> GetMergerGroups()
    {
        var groups = new Dictionary<string, List<string>>();
        foreach (var (oldNis, newNis) in Mergers)
        {
            if (!groups.TryGetValue(newNis, out var list))
            {
                list = [];
                groups[newNis] = list;
            }
            list.Add(oldNis);
        }
        return groups;
    }
}
