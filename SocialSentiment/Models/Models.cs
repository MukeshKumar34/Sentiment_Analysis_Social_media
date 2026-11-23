namespace SocialSentiment.Models;

public enum Sentiment { Negative, Neutral, Positive }

public record SocialPost(string Id, string Platform, string Author,
                         DateTime Timestamp, string Brand, string Text);

public record SentimentResult(string PostId, double Score, Sentiment Label,
                              double Confidence, IReadOnlyList<string> Keywords);

public record AnalyzeRequest(string Text);
public record BatchRequest(List<AnalyzeRequest> Items);

public record DailyTrend(DateOnly Date, int Posts, double AvgScore);
public record PlatformStat(string Platform, int Posts, double AvgScore);

public record BrandReport(
    string Brand, int TotalPosts,
    int Positive, int Neutral, int Negative,
    double PositivePct, double NegativePct,
    double AvgScore, double NetSentiment,
    string ReputationStatus,
    IReadOnlyList<DailyTrend> Trend,
    IReadOnlyList<PlatformStat> ByPlatform,
    IReadOnlyList<string> TopPositiveTopics,
    IReadOnlyList<string> TopNegativeTopics,
    IReadOnlyList<string> Recommendations);
