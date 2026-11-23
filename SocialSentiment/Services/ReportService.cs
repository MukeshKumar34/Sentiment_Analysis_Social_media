using SocialSentiment.Models;

namespace SocialSentiment.Services;

public class ReportService(LexiconSentimentAnalyzer analyzer)
{
    public IReadOnlyList<BrandReport> BuildReports(IEnumerable<SocialPost> posts) =>
        posts.GroupBy(p => p.Brand, StringComparer.OrdinalIgnoreCase)
             .Select(g => BuildReport(g.Key, g.ToList()))
             .OrderByDescending(r => r.TotalPosts).ToList();

    public BrandReport BuildReport(string brand, List<SocialPost> posts)
    {
        var scored = posts.Select(p => (Post: p, Res: analyzer.Analyze(p.Text, p.Id))).ToList();
        int total = scored.Count;
        int pos = scored.Count(s => s.Res.Label == Sentiment.Positive);
        int neg = scored.Count(s => s.Res.Label == Sentiment.Negative);
        int neu = total - pos - neg;
        double Pct(int n) => total == 0 ? 0 : Math.Round(100.0 * n / total, 1);
        double avg = total == 0 ? 0 : Math.Round(scored.Average(s => s.Res.Score), 3);
        double net = total == 0 ? 0 : Math.Round(100.0 * (pos - neg) / total, 1);

        var trend = scored.GroupBy(s => DateOnly.FromDateTime(s.Post.Timestamp))
            .OrderBy(g => g.Key)
            .Select(g => new DailyTrend(g.Key, g.Count(), Math.Round(g.Average(x => x.Res.Score), 3))).ToList();

        var byPlatform = scored.GroupBy(s => s.Post.Platform)
            .Select(g => new PlatformStat(g.Key, g.Count(), Math.Round(g.Average(x => x.Res.Score), 3)))
            .OrderByDescending(p => p.Posts).ToList();

        var topPos = TopTopics(scored, Sentiment.Positive);
        var topNeg = TopTopics(scored, Sentiment.Negative);

        string status = net >= 40 ? "Excellent" : net >= 15 ? "Healthy" : net >= -10 ? "Mixed" : "At Risk";
        return new BrandReport(brand, total, pos, neu, neg, Pct(pos), Pct(neg), avg, net, status,
                               trend, byPlatform, topPos, topNeg, Recommend(status, Pct(neg), topNeg, byPlatform));
    }

    private static List<string> TopTopics(List<(SocialPost Post, SentimentResult Res)> scored, Sentiment label) =>
        scored.Where(s => s.Res.Label == label)
              .SelectMany(s => s.Res.Keywords)
              .GroupBy(k => k).OrderByDescending(g => g.Count()).Take(5)
              .Select(g => $"{g.Key} ({g.Count()})").ToList();

    private static List<string> Recommend(string status, double negPct, List<string> negTopics, List<PlatformStat> plat)
    {
        var r = new List<string>();
        if (status == "At Risk") r.Add("ALERT: negative sentiment dominates. Start a rapid-response customer care campaign.");
        if (negPct > 30 && negTopics.Count > 0) r.Add($"Investigate recurring complaint themes: {string.Join(", ", negTopics.Take(3))}.");
        var worst = plat.OrderBy(p => p.AvgScore).FirstOrDefault();
        if (worst is { AvgScore: < 0 }) r.Add($"Prioritise community management on {worst.Platform} (lowest average sentiment).");
        if (status is "Healthy" or "Excellent") r.Add("Amplify positive posts: reshare testimonials and run advocacy campaigns.");
        if (r.Count == 0) r.Add("Sentiment is mixed; keep monitoring and A/B test messaging.");
        return r;
    }
}
