# Social Media Sentiment Analysis (C# / .NET 8)

Monitors public sentiment on social media posts, comments and interactions to assess brand reputation and
support marketing decisions.

## Architecture
| Layer | File | Responsibility |
|---|---|---|
| Ingestion | `Services/CsvPostLoader.cs` | Loads posts (CSV now; swap for Twitter/X, Reddit, Meta Graph API) |
| NLP preprocessing | `Services/TextPreprocessor.cs` | Lowercase, strip URLs/@mentions, hashtags, tokenise, stop-words |
| Sentiment engine | `Services/LexiconSentimentAnalyzer.cs` | Lexicon scoring + negation, intensifiers, emoji, caps/`!` emphasis → score in (-1,1) |
| Analytics | `Services/ReportService.cs` | Per-brand distribution, net sentiment, daily trend, per-platform stats, topics, alerts, recommendations |
| API | `Program.cs` | Minimal REST API |

## Run
```bash
dotnet run
```
Then open the printed URL, e.g. `http://localhost:5000/api/reports`.

## Endpoints
- `POST /api/analyze` body `{"text":"I love this brand!"}`
- `POST /api/analyze/batch` body `{"items":[{"text":"great"},{"text":"awful"}]}`
- `GET /api/posts` – sample posts with sentiment
- `GET /api/reports` and `GET /api/reports/Acme` – brand reputation reports

## Key metrics
- **Net Sentiment** = (%Positive − %Negative); status: Excellent ≥40, Healthy ≥15, Mixed ≥-10, At Risk below.
- Daily trend, platform comparison, top positive/negative topics, automated marketing recommendations.

## Extending
- Replace the analyzer with an ML.NET model (implement the same `Analyze` contract).
- Add a live connector class that returns `SocialPost` objects.
- Persist results with EF Core and add a dashboard (Blazor / Power BI).
