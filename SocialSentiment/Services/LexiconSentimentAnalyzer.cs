using SocialSentiment.Models;

namespace SocialSentiment.Services;

/// <summary>
/// Rule-based analyzer (VADER-inspired): word lexicon + negation + intensifiers
/// + emoji/emoticons + emphasis (ALL CAPS, '!').
/// </summary>
public class LexiconSentimentAnalyzer
{
    private static readonly Dictionary<string, double> Lexicon = new()
    {
        // positive
        ["love"]=3,["loved"]=3,["amazing"]=3,["excellent"]=3,["fantastic"]=3,["awesome"]=3,["perfect"]=3,
        ["great"]=2.5,["best"]=2.5,["happy"]=2.5,["wonderful"]=2.5,["recommend"]=2,["good"]=2,["nice"]=1.5,
        ["fast"]=1.5,["helpful"]=2,["reliable"]=2,["smooth"]=1.5,["easy"]=1.5,["impressed"]=2.5,["worth"]=1.5,
        ["thanks"]=1.5,["thank"]=1.5,["quality"]=1.5,["friendly"]=2,["improved"]=1.5,["win"]=2,["like"]=1,
        // negative
        ["hate"]=-3,["hated"]=-3,["terrible"]=-3,["awful"]=-3,["horrible"]=-3,["worst"]=-3,["scam"]=-3.5,
        ["bad"]=-2,["poor"]=-2,["disappointed"]=-2.5,["disappointing"]=-2.5,["slow"]=-1.5,["broken"]=-2.5,
        ["bug"]=-1.5,["buggy"]=-2,["crash"]=-2.5,["crashes"]=-2.5,["refund"]=-1.5,["rude"]=-2.5,["useless"]=-3,
        ["waste"]=-2.5,["expensive"]=-1.5,["overpriced"]=-2.5,["delay"]=-1.5,["delayed"]=-2,["fail"]=-2.5,
        ["failed"]=-2.5,["angry"]=-2.5,["annoying"]=-2,["problem"]=-1.5,["issue"]=-1.2,["cancel"]=-1.5,
        ["unhappy"]=-2.5,["complaint"]=-2,
        // emoticons / emoji
        [":)"]=2,[":-)"]=2,[":d"]=2.5,["😀"]=2.5,["😍"]=3,["👍"]=2,["❤"]=3,["😊"]=2,["🔥"]=1.5,
        [":("]=-2,[":-("]=-2,["😡"]=-3,["😠"]=-2.5,["👎"]=-2,["😢"]=-2,["😞"]=-2,["🤮"]=-3
    };

    private static readonly HashSet<string> Negators = new()
        { "not","no","never","isn't","wasn't","don't","doesn't","didn't","can't","cannot","won't","hardly","without","isnt","dont","cant" };

    private static readonly Dictionary<string, double> Boosters = new()
        { ["very"]=1.3,["really"]=1.3,["extremely"]=1.5,["super"]=1.4,["so"]=1.2,["absolutely"]=1.5,
          ["totally"]=1.3,["slightly"]=0.7,["somewhat"]=0.8,["barely"]=0.6 };

    public SentimentResult Analyze(string text, string postId = "")
    {
        var tokens = TextPreprocessor.Tokenize(text);
        double sum = 0;
        var hits = new List<string>();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (!Lexicon.TryGetValue(tokens[i], out var val)) continue;

            // look back up to 3 tokens for boosters / negators
            for (int k = 1; k <= 3 && i - k >= 0; k++)
            {
                var prev = tokens[i - k];
                if (Boosters.TryGetValue(prev, out var b)) val *= 1 + (b - 1) * (k == 1 ? 1 : 0.6);
                if (Negators.Contains(prev)) { val *= -0.74; break; }
            }
            sum += val;
            hits.Add(tokens[i]);
        }

        // emphasis: shouted words and exclamation marks amplify the existing polarity
        if (sum != 0)
        {
            int shouted = text.Split(' ').Count(w => w.Length > 2 && w.Any(char.IsLetter) && w == w.ToUpperInvariant());
            int excl = Math.Min(text.Count(c => c == '!'), 4);
            sum += Math.Sign(sum) * (shouted * 0.3 + excl * 0.25);
        }

        double score = sum / Math.Sqrt(sum * sum + 15);       // normalise to (-1, 1)
        var label = score >= 0.15 ? Sentiment.Positive : score <= -0.15 ? Sentiment.Negative : Sentiment.Neutral;
        double confidence = Math.Min(1, Math.Abs(score) * 1.5 + (hits.Count > 0 ? 0.1 : 0));

        var keywords = tokens.Where(t => t.Length > 2 && !TextPreprocessor.StopWords.Contains(t) && t.All(char.IsLetter))
                             .Distinct().Take(8).ToList();
        return new SentimentResult(postId, Math.Round(score, 3), label, Math.Round(confidence, 2), keywords);
    }
}
