using SocialSentiment.Models;
using SocialSentiment.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LexiconSentimentAnalyzer>();
builder.Services.AddSingleton<ReportService>();
var app = builder.Build();

var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "sample_posts.csv");

app.MapGet("/", () => Results.Ok(new
{
    name = "Social Media Sentiment Analysis API",
    endpoints = new[] { "POST /api/analyze", "POST /api/analyze/batch", "GET /api/reports", "GET /api/reports/{brand}", "GET /api/posts" }
}));

app.MapPost("/api/analyze", (AnalyzeRequest req, LexiconSentimentAnalyzer a) =>
    string.IsNullOrWhiteSpace(req.Text)
        ? Results.BadRequest("Text is required.")
        : Results.Ok(a.Analyze(req.Text)));

app.MapPost("/api/analyze/batch", (BatchRequest req, LexiconSentimentAnalyzer a) =>
    Results.Ok(req.Items.Select((x, i) => a.Analyze(x.Text, (i + 1).ToString()))));

app.MapGet("/api/posts", (LexiconSentimentAnalyzer a) =>
    Results.Ok(CsvPostLoader.Load(dataPath).Select(p => new { p, sentiment = a.Analyze(p.Text, p.Id) })));

app.MapGet("/api/reports", (ReportService rs) =>
    Results.Ok(rs.BuildReports(CsvPostLoader.Load(dataPath))));

app.MapGet("/api/reports/{brand}", (string brand, ReportService rs) =>
{
    var posts = CsvPostLoader.Load(dataPath)
        .Where(p => p.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase)).ToList();
    return posts.Count == 0 ? Results.NotFound($"No posts for brand '{brand}'.") : Results.Ok(rs.BuildReport(brand, posts));
});

app.Run();
