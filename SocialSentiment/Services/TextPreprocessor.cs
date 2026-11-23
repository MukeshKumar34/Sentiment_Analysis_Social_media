using System.Text.RegularExpressions;

namespace SocialSentiment.Services;

public static class TextPreprocessor
{
    private static readonly Regex Url = new(@"https?://\S+|www\.\S+", RegexOptions.Compiled);
    private static readonly Regex Mention = new(@"@\w+", RegexOptions.Compiled);
    private static readonly Regex Token = new(@"[a-z']+|[:;=]-?[()dp]|[^\u0000-\u007F]", RegexOptions.Compiled);

    public static readonly HashSet<string> StopWords = new()
    {
        "the","a","an","and","or","but","is","are","was","were","be","been","to","of","in","on","at",
        "for","with","it","its","this","that","i","me","my","we","you","they","he","she","so","as",
        "have","has","had","do","does","did","just","get","got","from","by","about","than","then"
    };

    /// <summary>Lowercases, strips URLs/@mentions, turns #hashtags into words, and tokenizes.</summary>
    public static List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();
        var t = Url.Replace(text.ToLowerInvariant(), " ");
        t = Mention.Replace(t, " ").Replace("#", " ");
        return Token.Matches(t).Select(m => m.Value.Trim('\'')).Where(s => s.Length > 0).ToList();
    }
}
