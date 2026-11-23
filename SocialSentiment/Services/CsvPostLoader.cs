using System.Globalization;
using SocialSentiment.Models;

namespace SocialSentiment.Services;

/// <summary>Loads posts from CSV: id,platform,author,timestamp,brand,text (text may be quoted).</summary>
public static class CsvPostLoader
{
    public static List<SocialPost> Load(string path)
    {
        var posts = new List<SocialPost>();
        if (!File.Exists(path)) return posts;
        foreach (var line in File.ReadLines(path).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var f = Split(line);
            if (f.Count < 6) continue;
            if (!DateTime.TryParse(f[3], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var ts)) continue;
            posts.Add(new SocialPost(f[0], f[1], f[2], ts, f[4], f[5]));
        }
        return posts;
    }

    private static List<string> Split(string line)
    {
        var res = new List<string>(); var sb = new System.Text.StringBuilder(); bool q = false;
        foreach (var c in line)
        {
            if (c == '"') q = !q;
            else if (c == ',' && !q) { res.Add(sb.ToString().Trim()); sb.Clear(); }
            else sb.Append(c);
        }
        res.Add(sb.ToString().Trim());
        return res;
    }
}
